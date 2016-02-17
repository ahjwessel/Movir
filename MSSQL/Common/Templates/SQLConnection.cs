using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace Common.Templates
{
    public abstract class SQLConnection: IDisposable
    {
        #region Properties
        internal protected DbConnection DBConnection { get; private set; }
        public string CurrentHostname { get; private set; }
        public int CurrentPort { get; private set; }
        public string CurrentUsername { get; private set; }
        public string CurrentPassword { get; private set; }
        public string SelectedDatabase { get; private set; }
        
        public Exception LastException{get; protected set;}
        public int TimeoutOpenRecordsetInSeconden { get; set; }
        public int TimeoutExecuteInSeconden { get; set; }
        public abstract string[] Databases { get; }
        public abstract string[] Tables { get; }
        public abstract DateTime SystemDateTime { get; }
        #endregion
        private void ResetLastException()
        {
            this.LastException = null;
        }

        internal protected abstract DbConnection CreateConnection();
        internal protected abstract DbCommand CreateCommand(string parSQL);
        internal protected abstract SQLRecordset CreateRecordset();

        #region OpenConnection/Reconnect/CloseConnection
        public bool OpenConnection(string parHost, int parPort,
                                   string parUsername, string parPassword)
        {
            return this.OpenConnection(parHost, parPort, parUsername, parPassword, "");
        }
        public bool OpenConnection(string parHost, int parPort,
                                   string parUsername, string parPassword,
                                   string parDatabase)
        {
            return this.OpenConnection(parHost, parPort, parUsername, parPassword, parDatabase, false);
        }
        protected bool OpenConnection(string parHost, int parPort,
                                      string parUsername, string parPassword,
                                      string parDatabase,
                                      bool parReconnectProcedure)
        {
            this.CloseConnection();
            this.CurrentHostname = parHost;
            this.CurrentPort = parPort;
            this.CurrentUsername = parUsername;
            this.CurrentPassword = parPassword;
            this.ResetLastException();

            try
            {
                this.DBConnection = this.CreateConnection();
                this.DBConnection.Open();

                while (this.DBConnection.State == ConnectionState.Connecting ||
                       this.DBConnection.State == ConnectionState.Executing ||
                       this.DBConnection.State == ConnectionState.Fetching)
                {
                    System.Threading.Thread.Sleep(1);
                }
                if (this.DBConnection.State == ConnectionState.Open)
                {
                    if (parDatabase != null && parDatabase != "")
                    {
                        if (!this.SelectDatabase(parDatabase))
                        {
                            if (!parReconnectProcedure)
                                throw new ApplicationException("Select database '" + parDatabase + "' in MySQL failed\n" + this.LastException.Message);

                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                if (!parReconnectProcedure)
                    throw ex;
            }

            return false;
        }
        public void Reconnect()
        {
            if (this.CurrentHostname != "" &&
                this.CurrentUsername != "" &&
                this.CurrentPassword != "")
            {
                this.OpenConnection(this.CurrentHostname,
                                    this.CurrentPort,
                                    this.CurrentUsername,
                                    this.CurrentPassword,
                                    this.SelectedDatabase,
                                    true);
            }
        }
        public void CloseConnection()
        {
            if (this.DBConnection != null)
            {
                this.DBConnection.Close();
                this.DBConnection.Dispose();
            }
        }
        #endregion

        #region CreateDatabase/SelectDatabase/Execute/OpenRecordset
        public abstract bool CreateDatabase(string parDatabaseName);
        public abstract bool SelectDatabase(string parDatabase);
        public int Execute(string parSQL)
        {
            int varReturn = int.MinValue;
            try
            {
                var cmd = this.CreateCommand(parSQL);
                cmd.CommandTimeout = this.TimeoutExecuteInSeconden;
                varReturn = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return varReturn;
        }
        public SQLRecordset OpenRecordset(string parSQL)
        {
            var rec = this.CreateRecordset();
            this.ResetLastException();
            try
            {
                if (!rec.OpenRecordset(parSQL))
                {
                    rec.Dispose();
                    rec = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            try
            {
                if (rec.Fields == null || rec.Fields.Count == 0)
                {
                    rec.Dispose();
                    rec = null;
                }
            }
            catch
            {
                if (rec != null)
                {
                    rec.Dispose();
                    rec = null;
                }
            }

            return rec;
        }
        #endregion

        #region Read X fields of Y records
        public object ReadFirstFieldOfFirstRecordSQL(string parSQL, object parDefaultValue)
        {
            object[] mtxValues = this.ReadAllFieldsOfFirstRecordSQL(parSQL, new object[] { parDefaultValue });
            return mtxValues[0];
        }
        public object[] ReadAllFieldsOfFirstRecordSQL(string parSQL, object[] parDefaultValues)
        {
            SQLRecordset rec;
            rec = this.OpenRecordset(parSQL);
            if (rec == null)
                return parDefaultValues;
            else if (rec.RecordCount == 0)
            {
                rec.Dispose();
                return parDefaultValues;
            }

            object[] mtxReturn;
            int varFieldCount;
            int varFieldCounter;

            varFieldCount = rec.Fields.Count;
            mtxReturn = new object[varFieldCount];

            //Setting the defaultvalues
            if (parDefaultValues != null)
            {
                for (varFieldCounter = 0; varFieldCounter <= varFieldCount - 1; varFieldCounter++)
                {
                    if (parDefaultValues.Length - 1 >= varFieldCounter)
                        mtxReturn[varFieldCounter] = parDefaultValues[varFieldCounter];
                }
            }

            //Reading this recordset
            if (rec != null)
            {
                if (!rec.EOF && rec.RecordCount > 0)
                {
                    rec.MoveFirst();
                    for (varFieldCounter = 1; varFieldCounter <= Math.Min(varFieldCount, rec.Fields.Count); varFieldCounter++)
                    {
                        if (Convert.ToString(rec.Fields[varFieldCounter].Value) + "" != "")
                        {
                            mtxReturn[varFieldCounter - 1] = rec.Fields[varFieldCounter].Value;
                        }
                    }
                }
                rec.Dispose();
                rec = null;
            }

            return mtxReturn;
        }
        public object[] ReadFirstFieldOfAllRecordSQL(string parSQL, object[] parDefaultValues)
        {
            object[] mtxReturn = null;
            SQLRecordset rec = this.OpenRecordset(parSQL);
            if (rec == null || rec.RecordCount == 0)
            {
                if (parDefaultValues != null)
                    mtxReturn = parDefaultValues;
            }
            else
            {
                mtxReturn = new object[rec.RecordCount];
                int varCurrentRecordIndex = 0;
                while (!rec.EOF)
                {
                    mtxReturn[varCurrentRecordIndex] = rec.Fields[1].Value;
                    rec.MoveNext();
                    varCurrentRecordIndex += 1;
                }
            }

            if (rec != null)
                rec.Dispose();

            return mtxReturn;
        }
        public object[,] ReadAllFieldsOfAllRecordSQL(string parSQL, object[] parDefaultValues)
        {
            SQLRecordset rec = this.OpenRecordset(parSQL);
            object[,] mtxReturn = null;
            if (rec == null || rec.RecordCount == 0)
            {
                if (parDefaultValues != null)
                {
                    mtxReturn = new object[parDefaultValues.Length, 1];
                    for (int varCounter = 0; varCounter < parDefaultValues.Length; varCounter++)
                    {
                        mtxReturn[varCounter, 0] = parDefaultValues[varCounter];
                    }
                }
            }
            else
                mtxReturn = rec.getRows();

            if (rec != null)
                rec.Dispose();

            return mtxReturn;
        }
        #endregion

        #region Check function
        public bool HasDatabase(string parName)
        {
            foreach (string varTableName in this.Databases)
            {
                if (varTableName.ToLower() == parName.ToLower())
                    return true;
            }
            return false;
        }
        public bool HasTable(string parName)
        {
            string[] varTableNames = this.Tables;
            if (varTableNames != null)
            {
                foreach (string varTableName in varTableNames)
                {
                    if (varTableName.ToLower() == parName.ToLower())
                        return true;
                }
            }
            return false;
        }
        public bool HasNoRecords(string parSQL)
        {
            SQLRecordset rec = this.OpenRecordset(parSQL);
            if (rec != null)
            {
                bool varReturnValue = rec.EOF;
                rec.Dispose();
                return varReturnValue;
            }

            return true;
        }
        #endregion

        public static string getEnumToTextSubquery(string parFieldname,
                                           bool parEnumvalueAsText,
                                           Type parEnum,
                                           string parUnknownText)
        {
            //CASE value WHEN [compare_value] THEN result [WHEN [compare_value] THEN result ...] [ELSE result] END 
            string varReturn = "";
            foreach (object EnumValue in Enum.GetValues(parEnum))
            {
                if (parEnumvalueAsText)
                    varReturn += "WHEN \"" + ((char) Convert.ToInt32(EnumValue)).ToString() + "\"";
                else
                    varReturn += "WHEN " + Convert.ToInt32(EnumValue).ToString();
                varReturn += " THEN \"" + Enum.GetName(parEnum, EnumValue) + "\" ";
            }
            varReturn = "(CASE " + parFieldname + " " + varReturn + "ELSE \"" + parUnknownText + "\" END)";
            return varReturn;
        }

        public virtual void Dispose()
        {
            this.CloseConnection();
            this.CurrentHostname = null;
            this.CurrentPassword = null;
            this.CurrentUsername = null;
        }

        public SQLConnection()
        {
            this.TimeoutOpenRecordsetInSeconden = 30;
            this.TimeoutExecuteInSeconden = 180;
        }
    }
}
