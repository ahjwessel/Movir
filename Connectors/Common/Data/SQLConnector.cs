using System;
using System.Data;
using System.Data.Common;

namespace Common.Data
{
    public abstract class SQLConnector: IDisposable
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
        internal protected abstract DbCommand CreateCommand(string SQL);
        internal protected abstract SQLRecordset CreateRecordset();

        #region OpenConnection/Reconnect/CloseConnection
        public bool OpenConnection(string host, int port,
                                   string username, string password)
        {
            return this.OpenConnection(host, port, username, password, "");
        }
        public bool OpenConnection(string host, int port,
                                   string username, string password,
                                   string database)
        {
            return this.OpenConnection(host, port, username, password, database, false);
        }
        protected bool OpenConnection(string host, int port,
                                      string username, string password,
                                      string database,
                                      bool reconnectProcedure)
        {
            this.CloseConnection();
            this.CurrentHostname = host;
            this.CurrentPort = port;
            this.CurrentUsername = username;
            this.CurrentPassword = password;
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
                    if (database != null && database != "")
                    {
                        if (!this.SelectDatabase(database))
                        {
                            if (!reconnectProcedure)
                                throw new ApplicationException("Select database '" + database + "' in MySQL failed\n" + this.LastException.Message);

                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                if (!reconnectProcedure)
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
                this.DBConnection = null;
            }
        }
        #endregion

        #region CreateDatabase/SelectDatabase/DeleteTable/Execute/OpenRecordset
        public bool CreateDatabase(string databaseName)
        {
            if (this.PCreateDatabase(databaseName))
            {
                this.SelectedDatabase = databaseName;
                return true;
            }
            else
                return false;
        }
        protected abstract bool PCreateDatabase(string databaseName);
        public bool SelectDatabase(string database)
        {
            if (this.pSelectDatabase(database))
            {
                this.SelectedDatabase = database;
                return true;
            }
            else
                return false;
        }
        protected abstract bool pSelectDatabase(string database);
        public abstract void DeleteTable(string tablename);
        public int Execute(string SQL)
        {
            if (SQL == "")
                return int.MinValue;

            int returnValue = int.MinValue;
            using (var command = this.CreateCommand(SQL))
            {
                try
                {
                    command.CommandTimeout = this.TimeoutExecuteInSeconden;
                    returnValue = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return returnValue;
        }
        public SQLRecordset OpenRecordset(string SQL)
        {
            var rec = this.CreateRecordset();
            this.ResetLastException();
            try
            {
                if (!rec.OpenRecordset(SQL))
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
        public object ReadFirstFieldOfFirstRecordSQL(string SQL, object parDefaultValue)
        {
            object[] mtxValues = this.ReadAllFieldsOfFirstRecordSQL(SQL, new object[] { parDefaultValue });
            return mtxValues[0];
        }
        public object[] ReadAllFieldsOfFirstRecordSQL(string SQL, object[] defaultValues)
        {
            SQLRecordset rec;
            rec = this.OpenRecordset(SQL);
            if (rec == null)
                return defaultValues;
            else if (rec.RecordCount == 0)
            {
                rec.Dispose();
                return defaultValues;
            }

            object[] returnValue;
            int varFieldCount;
            int varFieldCounter;

            varFieldCount = rec.Fields.Count;
            returnValue = new object[varFieldCount];

            //Setting the defaultvalues
            if (defaultValues != null)
            {
                for (varFieldCounter = 0; varFieldCounter <= varFieldCount - 1; varFieldCounter++)
                {
                    if (defaultValues.Length - 1 >= varFieldCounter)
                        returnValue[varFieldCounter] = defaultValues[varFieldCounter];
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
                            returnValue[varFieldCounter - 1] = rec.Fields[varFieldCounter].Value;
                    }
                }
                rec.Dispose();
                rec = null;
            }

            return returnValue;
        }
        public object[] ReadFirstFieldOfAllRecordSQL(string SQL, object[] defaultValues)
        {
            object[] returnValue = null;
            SQLRecordset rec = this.OpenRecordset(SQL);
            if (rec == null || rec.RecordCount == 0)
            {
                if (defaultValues != null)
                    returnValue = defaultValues;
            }
            else
            {
                returnValue = new object[rec.RecordCount];
                int varCurrentRecordIndex = 0;
                while (!rec.EOF)
                {
                    returnValue[varCurrentRecordIndex] = rec.Fields[1].Value;
                    rec.MoveNext();
                    varCurrentRecordIndex += 1;
                }
            }

            if (rec != null)
                rec.Dispose();

            return returnValue;
        }
        public object[,] ReadAllFieldsOfAllRecordSQL(string SQL, object[] defaultValues)
        {
            SQLRecordset rec = this.OpenRecordset(SQL);
            object[,] returnValue = null;
            if (rec == null || rec.RecordCount == 0)
            {
                if (defaultValues != null)
                {
                    returnValue = new object[defaultValues.Length, 1];
                    for (int counter = 0; counter < defaultValues.Length; counter++)
                    {
                        returnValue[counter, 0] = defaultValues[counter];
                    }
                }
            }
            else
                returnValue = rec.GetRows();

            if (rec != null)
                rec.Dispose();

            return returnValue;
        }
        #endregion

        #region Check function
        public bool HasDatabase(string name)
        {
            var DatabaseNames = this.Databases;
            if (DatabaseNames != null)
            {
                foreach (string DatabaseName in DatabaseNames)
                {
                    if (DatabaseName.ToLower() == name.ToLower())
                        return true;
                }
            }
            return false;
        }
        public bool HasTable(string name)
        {
            var tablenames = this.Tables;
            if (tablenames != null)
            {
                foreach (string tablename in tablenames)
                {
                    if (tablename.ToLower() == name.ToLower())
                        return true;
                }
            }
            return false;
        }
        public bool HasNoRecords(string SQL)
        {
            bool ReturnValue = false;
            using (var rec = this.OpenRecordset(SQL))
            {
                if (rec != null)
                {
                    ReturnValue = rec.EOF;
                    rec.Dispose();
                }
            }

            return ReturnValue;
        }
        #endregion

        public static string GetEnumToTextSubquery(string fieldname,
                                                   bool enumvalueAsText,
                                                   Type enumType,
                                                   string UnknownText)
        {
            //CASE value WHEN [compare_value] THEN result [WHEN [compare_value] THEN result ...] [ELSE result] END 
            string returnValue = "";
            foreach (object EnumValue in Enum.GetValues(enumType))
            {
                if (enumvalueAsText)
                    returnValue += "WHEN \"" + ((char) Convert.ToInt32(EnumValue)).ToString() + "\"";
                else
                    returnValue += "WHEN " + Convert.ToInt32(EnumValue).ToString();
                returnValue += " THEN \"" + Enum.GetName(enumType, EnumValue) + "\" ";
            }
            returnValue = "(CASE " + fieldname + " " + returnValue + "ELSE \"" + UnknownText + "\" END)";
            return returnValue;
        }

        public virtual void Dispose()
        {
            this.CloseConnection();
            this.CurrentHostname = null;
            this.CurrentPassword = null;
            this.CurrentUsername = null;
        }

        public SQLConnector()
        {
            this.TimeoutOpenRecordsetInSeconden = 30;
            this.TimeoutExecuteInSeconden = 180;
        }
    }
}
