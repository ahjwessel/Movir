using System;
using System.Data.Common;
using System.Globalization;
using Common.Data;
using System.Data;

namespace MSSQL
{
    public class MSSQLConnector : SQLConnector
    {
        #region Properties
        public override string[] Databases
        {
            get
            {
                var rec = this.OpenRecordset("SELECT name FROM master.dbo.sysdatabases");
                if (rec == null)
                    return null;

                if (rec.RecordCount==0)
                {
                    rec.Dispose();
                    return null;
                }

                var returnValue = new string[rec.RecordCount];
                var rowValue = rec.GetRows();

                for (int counter=0;counter<returnValue.Length;counter++)
                {
                    returnValue[counter] = rowValue[0, counter].ToString();
                }

                return returnValue;
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
                var rec = this.OpenRecordset("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.Tables");
                if (rec == null)
                    return null;

                if (rec.RecordCount == 0)
                {
                    rec.Dispose();
                    return null;
                }

                var returnValue = new string[rec.RecordCount];
                var rowValue = rec.GetRows();

                for (int counter = 0; counter < returnValue.Length; counter++)
                {
                    returnValue[counter] = rowValue[0, counter].ToString();
                }

                return returnValue;
            }
        }
        #endregion

        #region Protected
        protected override bool PCreateDatabase(string databaseName)
        {
            try
            {
                this.Execute("CREATE DATABASE " + databaseName);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        protected override bool pSelectDatabase(string database)
        {
            try
            {
                this.Execute("USE " + database);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        protected internal override DbCommand CreateCommand(string SQL)
        {
            return new System.Data.SqlClient.SqlCommand(SQL,(System.Data.SqlClient.SqlConnection)this.DBConnection);
        }
        protected internal override DbConnection CreateConnection()
        {
            if (this.CurrentUsername=="")
                return new System.Data.SqlClient.SqlConnection("Server=" + this.CurrentHostname + "; persist security info=True; Integrated Security = SSPI;");
            else
                return new System.Data.SqlClient.SqlConnection("Server=" + this.CurrentHostname + "; User Id=" + this.CurrentUsername + "; Password=" + this.CurrentPassword);
        }
        protected internal override SQLRecordset CreateRecordset()
        {
            return new MSSQLRecordset(this);
        }
        #endregion

        #region ConvertValueToSQL/ConvertSQLToValue
        public static string ConvertValueToSQL(SqlDbType SQLType,object value,bool allowDBNull)
        {
            switch (SQLType)
            {
                case SqlDbType.BigInt:
                case SqlDbType.Bit:
                case SqlDbType.Int:
                case SqlDbType.SmallInt:
                case SqlDbType.TinyInt:
                    if (value == null)
                        return "0";
                    else if (value is bool)
                        return ((bool)value ? "1" : "0");
                    else
                        return Convert.ToString(value);
                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Money:
                case SqlDbType.Real:
                case SqlDbType.SmallMoney:
                    if (value == null)
                        return "0";
                    else
                        return value.ToString().Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, "").Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
                case SqlDbType.Date:
                    DateTime date;
                    DateTime varMinDate = new DateTime(1753, 1, 1);
                    if (value == null || (DateTime)value < varMinDate)
                        date = varMinDate;
                    else
                        date = ((DateTime)value).Date;
                    return "'" + date.Year.ToString() + "-" + date.Month.ToString("00") + "-" + date.Day.ToString("00") + "'";
                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    DateTime dateTime;
                    DateTime varMinDateTime = SQLType == SqlDbType.DateTime ? new DateTime(1753, 1, 1) : new DateTime(1900, 1, 1);
                    DateTime varMaxDateTime = SQLType == SqlDbType.DateTime ? DateTime.MaxValue : new DateTime(2079, 6, 6);
                    if (value == null)
                        dateTime = varMinDateTime;
                    else if ((DateTime)value > varMaxDateTime)
                        dateTime = varMaxDateTime;
                    else
                        dateTime = ((DateTime)value);

                    return "'" + dateTime.Year.ToString() + "-" + dateTime.Month.ToString("00") + "-" + dateTime.Day.ToString("00") +
                                 " " +
                                 dateTime.Hour.ToString("00") + ":" + dateTime.Minute.ToString("00") + ":" + dateTime.Second.ToString("00") +
                           "'";
                case SqlDbType.Time:
                    DateTime varTime = (DateTime)value;
                    return "'" + varTime.Hour.ToString("00") + ":" + varTime.Minute.ToString("00") + ":" + varTime.Second.ToString("00") + "'";
                case SqlDbType.UniqueIdentifier:
                    return "'" + ((Guid)value).ToString() + "'";
                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.VarBinary:
                    if (value is byte[])
                    {
                        return "0x" + BitConverter.ToString((byte[])value).Replace("-", string.Empty);
                    }
                    else if (value == null)
                    {
                        if (allowDBNull)
                            return "null";
                        else
                            return "''";
                    }
                    else
                    {
                        return "'" + value.ToString() + "'";
                    }
                default:
                    if (value == null)
                        return "''";
                    else
                        return "'" + ((string)value).Replace("'", "''").Replace("\\", "\\\\") + "'";
            }
        }
        public static object ConvertSQLToValue(SqlDbType SQLType, object value)
        {
            switch (SQLType)
            {
                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.VarBinary:
                    try
                    {
                        if (value == null)
                            return null;
                        else
                        {
                            byte[] mtx = (byte[])value;
                            if (mtx.Length == 0)
                                return null;
                            else
                                return mtx;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                case SqlDbType.BigInt:
                    return Convert.ToInt64(value);
                case SqlDbType.Bit:
                    return Convert.ToBoolean(value);
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Time:
                    return Convert.ToDateTime(value);
                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return Convert.ToDecimal(value);
                case SqlDbType.Float:
                    return Convert.ToDouble(value);
                case SqlDbType.Int:
                    return Convert.ToInt32(value);
                case SqlDbType.Real:
                    return Convert.ToSingle(value);
                case SqlDbType.SmallInt:
                    return Convert.ToInt16(value);
                case SqlDbType.TinyInt:
                    return Convert.ToByte(value);
                default:
                    return Convert.ToString(value);
            }
        }
        #endregion

        #region Publics
        public override void DeleteTable(string tablename)
        {
            string sqlstr = "IF OBJECT_ID('dbo.~', 'U') IS NOT NULL DROP TABLE dbo.~".Replace("~",tablename);
            this.Execute(sqlstr);
        }
        #endregion
    }
}
