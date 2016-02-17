using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Templates;

namespace MSSQL
{
    public class MSSQLConnection : SQLConnection
    {
        public override string[] Databases
        {
            get
            {
                var rec = this.OpenRecordset("SHOW DATABASES");
                if (rec==null || rec.RecordCount==0)
                {
                    rec.Dispose();
                    return null;
                }

                var mtxReturn = new string[rec.RecordCount];
                var mtxRows = rec.getRows();

                for (int varCounter=0;varCounter<mtxReturn.Length;varCounter++)
                {
                    mtxReturn[varCounter] = mtxRows[0, varCounter].ToString();
                }

                return mtxReturn;
            }
        }

        public override DateTime SystemDateTime
        {
            get
            {
                return (DateTime)this.ReadFirstFieldOfFirstRecordSQL("SELECT CURDATE()", DateTime.MinValue);
            }
        }

        public override string[] Tables
        {
            get
            {
                var rec = this.OpenRecordset("SHOW TABLES");
                if (rec == null || rec.RecordCount == 0)
                {
                    rec.Dispose();
                    return null;
                }

                var mtxReturn = new string[rec.RecordCount];
                var mtxRows = rec.getRows();

                for (int varCounter = 0; varCounter < mtxReturn.Length; varCounter++)
                {
                    mtxReturn[varCounter] = mtxRows[0, varCounter].ToString();
                }

                return mtxReturn;
            }
        }

        public override bool CreateDatabase(string parDatabaseName)
        {
            try
            {
                this.Execute("CREATE DATABASE " + parDatabaseName);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override bool SelectDatabase(string parDatabase)
        {
            try
            {
                this.Execute("USE DATABASE " + parDatabase);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected internal override DbCommand CreateCommand(string parSQL)
        {
            return new System.Data.SqlClient.SqlCommand(parSQL,(System.Data.SqlClient.SqlConnection)this.DBConnection);
        }

        protected internal override DbConnection CreateConnection()
        {
            return new System.Data.SqlClient.SqlConnection("Server=" + this.CurrentHostname + "; User Id=" + this.CurrentUsername + "; Password=" + this.CurrentPassword);
        }

        protected internal override SQLRecordset CreateRecordset()
        {
            return new MSSQLRecordset(this);
        }
    }
}
