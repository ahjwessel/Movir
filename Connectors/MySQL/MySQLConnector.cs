using System;
using System.Data.Common;
using System.Globalization;
using Common.Data;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace MySQL
{
    #region Enums
    public enum MySQLShows : byte
    {
        MY_SHOW_DATABASES,
        MY_SHOW_TABLES,
        MY_SHOW_COLUMNS,
        MY_SHOW_INDEX,
        MY_SHOW_TABLE_STATUS,
        MY_SHOW_STATUS,
        MY_SHOW_VARIABLES,
        MY_SHOW_LOGS,
        MY_SHOW_PROCESSLIST,
        MY_SHOW_GRANTS_FOR,
        MY_SHOW_CREATE_TABLE,
        MY_SHOW_MASTER_STATUS,
        MY_SHOW_MASTER_LOGS,
        MY_SHOW_SLAVE_STATUS,
        MY_SHOW_TABLE_TYPES,
        MY_SHOW_PRIVILEGES
    }
    public enum MySQLLocktypes : byte
    {
        MY_LOCK_READ,
        MY_LOCK_READ_LOCAL,
        MY_LOCK_WRITE,
        MY_LOCK_LOW_PRIORITY_WRITE
    }
    public enum MySQLDateTimeLanguages
    {
        English,
        Dutch
    }
    #endregion
    public class MySQLConnector : SQLConnector
    {
        #region Properties
        public override string[] Databases
        {
            get
            {
                var rec = this.Show(MySQLShows.MY_SHOW_DATABASES);
                if (rec == null)
                    return null;

                if (rec.RecordCount == 0)
                {
                    rec.Dispose();
                    return null;
                }

                var returnValue = new string[rec.RecordCount];
                var rowValues = rec.GetRows();

                for (int counter = 0; counter < returnValue.Length; counter++)
                {
                    returnValue[counter] = rowValues[0, counter].ToString();
                }

                return returnValue;
            }
        }
        public override DateTime SystemDateTime
        {
            get
            {
                return (DateTime)this.ReadFirstFieldOfFirstRecordSQL("SELECT SYSDATE()", DateTime.MinValue);
            }
        }
        public override string[] Tables
        {
            get
            {
                var rec = this.Show(MySQLShows.MY_SHOW_TABLES);
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

        #region Overrides
        protected override bool PCreateDatabase(string databaseName)
        {
            this.Execute("CREATE DATABASE " + databaseName);
            return true;
        }
        protected override bool pSelectDatabase(string database)
        {
            this.Execute("USE " + database);
            return true;
        }
        protected internal override DbCommand CreateCommand(string SQL)
        {
            return new MySqlCommand(SQL, (MySqlConnection)this.DBConnection);
        }
        protected internal override DbConnection CreateConnection()
        {
            return new MySqlConnection("server=" + this.CurrentHostname +
                                      ";port=" + this.CurrentPort.ToString() +
                                      ";user=" + this.CurrentUsername +
                                      ";password=" + this.CurrentPassword +
                                      ";Allow User Variables=True;");
        }
        protected internal override SQLRecordset CreateRecordset()
        {
            return new MySQLRecordset(this);
        }
        public override void DeleteTable(string tablename)
        {
            string sqlstr = "DROP TABLE IF EXISTS `" + tablename + "`";
            this.Execute(sqlstr);
        }
        #endregion

        #region ConvertValueToSQL/ConvertSQLToValue
        public static string ConvertValueToSQL(MySqlDbType SQLType, object value, bool allowDBNull)
        {
            switch (SQLType)
            {
                case MySqlDbType.Bit:
                case MySqlDbType.Byte:
                case MySqlDbType.Int16:
                case MySqlDbType.Int24:
                case MySqlDbType.Int32:
                case MySqlDbType.Int64:
                case MySqlDbType.UByte:
                case MySqlDbType.UInt16:
                case MySqlDbType.UInt24:
                case MySqlDbType.UInt32:
                case MySqlDbType.UInt64:
                case MySqlDbType.Year:
                    if (value == null)
                        return "0";
                    else if (value is bool)
                        return ((bool)value ? "1" : "0");
                    else
                        return Convert.ToString(value);
                case MySqlDbType.Decimal:
                case MySqlDbType.Double:
                case MySqlDbType.Float:
                    if (value == null)
                        return "0";
                    else
                        return value.ToString().Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, "").Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
                case MySqlDbType.Date:
                case MySqlDbType.Newdate:
                    DateTime date = (DateTime)value;
                    return "'" + date.Year.ToString() + "-" + date.Month.ToString("00") + "-" + date.Day.ToString("00") + "'";
                case MySqlDbType.DateTime:
                    DateTime dateTime = (DateTime)value;
                    return "'" + dateTime.Year.ToString() + "-" + dateTime.Month.ToString("00") + "-" + dateTime.Day.ToString("00") +
                                 " " +
                                 dateTime.Hour.ToString("00") + ":" + dateTime.Minute.ToString("00") + ":" + dateTime.Second.ToString("00") +
                           "'";
                case MySqlDbType.Time:
                    DateTime time;
                    if (value is TimeSpan)
                        time = DateTime.Now.Add((TimeSpan)value);
                    else
                        time = (DateTime)value;
                    return "'" + time.Hour.ToString("00") + ":" + time.Minute.ToString("00") + ":" + time.Second.ToString("00") + "'";
                case MySqlDbType.Blob:
                case MySqlDbType.LongBlob:
                case MySqlDbType.MediumBlob:
                case MySqlDbType.TinyBlob:
                case MySqlDbType.VarBinary:
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
                case MySqlDbType.Guid:
                    return "'" + ((Guid)value).ToString() + "'";
                default:
                    if (value == null)
                        return "''";
                    else
                        return "'" + ((string)value).Replace("'", "''").Replace("\\", "\\\\") + "'";
            }
        }
        public static object ConvertSQLToValue(MySqlDbType SQLType, object value)
        {
            switch (SQLType)
            {
                case MySqlDbType.Binary:
                case MySqlDbType.Blob:
                case MySqlDbType.LongBlob:
                case MySqlDbType.MediumBlob:
                case MySqlDbType.TinyBlob:
                case MySqlDbType.VarBinary:
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
                case MySqlDbType.Bit:
                    return Convert.ToBoolean(value);
                case MySqlDbType.Byte:
                case MySqlDbType.UByte:
                    return Convert.ToByte(value);
                case MySqlDbType.Date:
                case MySqlDbType.DateTime:
                case MySqlDbType.Newdate:
                case MySqlDbType.Time:
                case MySqlDbType.Timestamp:
                    return Convert.ToDateTime(value);
                case MySqlDbType.Decimal:
                    return Convert.ToDecimal(value);
                case MySqlDbType.Double:
                    return Convert.ToDouble(value);
                case MySqlDbType.Float:
                    return Convert.ToSingle(value);
                case MySqlDbType.Int16:
                    return Convert.ToInt16(value);
                case MySqlDbType.Int24:
                case MySqlDbType.Int32:
                    return Convert.ToInt32(value);
                case MySqlDbType.Int64:
                    return Convert.ToInt64(value);
                case MySqlDbType.UInt16:
                    return Convert.ToUInt16(value);
                case MySqlDbType.UInt24:
                case MySqlDbType.UInt32:
                    return Convert.ToUInt32(value);
                case MySqlDbType.UInt64:
                    return Convert.ToUInt64(value);
                case MySqlDbType.Year:
                    return Convert.ToInt16(value);
                default:
                    return Convert.ToString(value);
            }
        }
        #endregion

        #region Specifieke MySQL functies
        public int MaximumBinaryFieldSize
        {
            get
            {
                int varReturn = Convert.ToInt32(this.ReadAllFieldsOfFirstRecordSQL("SHOW VARIABLES LIKE \"max_allowed_packet\"", new object[] { "", 0 })[1]);
                varReturn -= 10000;
                if (varReturn < 0)
                    varReturn = 0;
                else
                    varReturn /= 2;

                return varReturn;
            }
        }
        public void CleanTable(string tableName)
        {
            this.Execute("TRUNCATE TABLE `" + tableName + "`");
        }
        /*
        #region Backup/Restore
        public const string BackupErrorLog = "backup.err";
        public const string CreateBackupExe = "mysqldump.exe";
        [Description("To use this function, the program 'mysqldump' should be present in the application folder.")]
        public static bool Backup(string parHostname, int parPort,
                                  string parUsername, string parPassword,
                                  string parDatabase, string parFilePath,
                                  params string[] parExcludeTables)
        {
            string varTempBat = FileTools.GenerateTempfileName("MySQL_Backup", "bat");
            string varLogFile = FileTools.PutAppPathToFile(BackupErrorLog);
            string varMySQLDump = FileTools.PutAppPathToFile(CreateBackupExe);
            string varParams = "--host=" + parHostname + " " +
                             "--port=" + parPort.ToString() + " " +
                             "--user=" + parUsername + " " +
                             "--password=" + parPassword + " " +
                             "--add-drop-database " +
                             parDatabase;
            string varTempDumpExe = FileTools.getFolderOfPath(varTempBat) + CreateBackupExe;

            if (!FileTools.FindFile(varMySQLDump))
            {
                MessageBox.Show(null, "Error while backuping `" + parDatabase + "`:\n" + varMySQLDump + " doesn't exist!", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            if (parExcludeTables != null &&
                parExcludeTables.Length > 0)
            {
                foreach (string varExcludeTable in parExcludeTables)
                {
                    varParams += " --ignore-table=" + parDatabase + "." + varExcludeTable;
                }
            }
            varParams += " > \"" + varTempBat + ".sql\"";

            try
            {
                if (FileTools.FindFile(varLogFile))
                    System.IO.File.Delete(varLogFile);

                if (!FileTools.FindFile(varTempDumpExe))
                    System.IO.File.Copy(varMySQLDump, varTempDumpExe);


                FileTools.AddTextToTextfile(varTempBat, "\"" + varTempDumpExe + "\" " + varParams);
                FileTools.ShellExecute(varTempBat, "", System.Diagnostics.ProcessWindowStyle.Minimized, "", FileTools.getFolderOfPath(varTempBat), true).WaitForExit();
                System.IO.File.Delete(varTempBat);

                if (FileTools.FindFile(varTempBat + ".sql"))
                    System.IO.File.Move(varTempBat + ".sql", parFilePath);

                try
                {
                    System.IO.File.Delete(varTempDumpExe);
                }
                catch { }

                if (!FileTools.FindFile(varLogFile))
                    return true;
            }
            catch (Exception ex)
            {
                ex.GetType();
                //MessageBox.Show(Form.ActiveForm, "Error while backuping `" + parDatabase + "`:\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return false;
        }
        public const string RestoreBackupExe = "mysql.exe";
        [Description("To use this function, the program 'mysql' should be present in the application folder.")]
        public static bool Restore(string parHostname, int parPort,
                                   string parUsername, string parPassword,
                                   string parDestinationDatabase, string parFilePath,
                                   bool parDropDatabase)
        {
            string varTempBat = FileTools.GenerateTempfileName("MySQL_Backup", "bat");
            string varLogFile = FileTools.PutAppPathToFile(BackupErrorLog);
            string varMySQLExe = FileTools.PutAppPathToFile(RestoreBackupExe);
            string varTempMySQLExe = FileTools.getFolderOfPath(varTempBat) + RestoreBackupExe;
            string varTempFilePath = FileTools.getFolderOfPath(varTempBat) + FileTools.getFileOfPath(parFilePath);
            string varParams = "--host=" + parHostname + " " +
                             "--port=" + parPort.ToString() + " " +
                             "--user=" + parUsername + " " +
                             "--password=" + parPassword + " " +
                             parDestinationDatabase + " " +
                             "< \"" + varTempFilePath + "\"";

            if (!FileTools.FindFile(varMySQLExe))
            {
                MessageBox.Show(null, "Error while restoring `" + parDestinationDatabase + "`:\n" + varMySQLExe + " doesn't exist!", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            try
            {
                if (parDropDatabase)
                {
                    typMYSQLConnection sql = new typMYSQLConnection();
                    sql.ErrorType = enmErrorType.ThrowError;
                    if (sql.OpenConnection(parHostname, parPort, parUsername, parPassword))
                    {
                        if (sql.HasDatabase(parDestinationDatabase))
                        {
                            try
                            {
                                sql.Execute("DROP DATABASE " + parDestinationDatabase);
                            }
                            catch { }
                        }

                        sql.CreateDatabase(parDestinationDatabase);
                    }
                    sql.Dispose();
                }

                if (FileTools.FindFile(varLogFile))
                    System.IO.File.Delete(varLogFile);

                if (!FileTools.FindFile(varTempMySQLExe))
                    System.IO.File.Copy(varMySQLExe, varTempMySQLExe);

                bool varTempCopyGelukt = false;
                try
                {
                    System.IO.File.Copy(parFilePath, varTempFilePath);
                    varTempCopyGelukt = true;
                }
                catch
                {
                    varTempFilePath = parFilePath;
                }
                FileTools.AddTextToTextfile(varTempBat, "\"" + varTempMySQLExe + "\" " + varParams);
                FileTools.ShellExecute(varTempBat, System.Diagnostics.ProcessWindowStyle.Minimized, true).WaitForExit();
                System.IO.File.Delete(varTempBat);
                if (varTempCopyGelukt)
                    System.IO.File.Delete(varTempFilePath);

                try
                {
                    System.IO.File.Delete(varTempMySQLExe);
                }
                catch { }

                if (!FileTools.FindFile(varLogFile))
                    return true;
            }
            catch (Exception ex)
            {
                ex.GetType();
                //MessageBox.Show(null, "Error while restoring `" + parDestinationDatabase + "`:\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return false;
        }
        #endregion
        */
        #region Show
        public MySQLRecordset Show(MySQLShows showType)
        {
            return this.Show(showType, "");
        }
        public MySQLRecordset Show(MySQLShows showType, string tableName)
        {
            string varCommand = "";

            //Determine Show Type
            switch (showType)
            {
                case MySQLShows.MY_SHOW_DATABASES:
                    varCommand = "DATABASES ";
                    break;
                case MySQLShows.MY_SHOW_TABLES:
                    varCommand = "TABLES ";
                    break;
                case MySQLShows.MY_SHOW_COLUMNS:
                    varCommand = "COLUMNS FROM ";
                    break;
                case MySQLShows.MY_SHOW_INDEX:
                    varCommand = "INDEX FROM ";
                    break;
                case MySQLShows.MY_SHOW_TABLE_STATUS:
                    varCommand = "TABLE STATUS ";
                    break;
                case MySQLShows.MY_SHOW_STATUS:
                    varCommand = "STATUS ";
                    break;
                case MySQLShows.MY_SHOW_VARIABLES:
                    varCommand = "VARIABLES ";
                    break;
                case MySQLShows.MY_SHOW_LOGS:
                    varCommand = "LOGS ";
                    break;
                case MySQLShows.MY_SHOW_PROCESSLIST:
                    varCommand = "PROCESSLIST";
                    break;
                case MySQLShows.MY_SHOW_GRANTS_FOR:
                    varCommand = "GRANTS FOR ";
                    break;
                case MySQLShows.MY_SHOW_CREATE_TABLE:
                    varCommand = "CREATE TABLE ";
                    break;
                case MySQLShows.MY_SHOW_MASTER_STATUS:
                    varCommand = "MASTER STATUS ";
                    break;
                case MySQLShows.MY_SHOW_MASTER_LOGS:
                    varCommand = "MASTER LOGS ";
                    break;
                case MySQLShows.MY_SHOW_SLAVE_STATUS:
                    varCommand = "SLAVE STATUS ";
                    break;
                case MySQLShows.MY_SHOW_TABLE_TYPES:
                    varCommand = "TABLE TYPES ";
                    break;
                case MySQLShows.MY_SHOW_PRIVILEGES:
                    varCommand = "PRIVILEGES ";
                    break;
            }

            //Check for a table name
            switch (showType)
            {
                case MySQLShows.MY_SHOW_COLUMNS:
                case MySQLShows.MY_SHOW_INDEX:
                case MySQLShows.MY_SHOW_CREATE_TABLE:
                    if (tableName != null && tableName != "")
                        varCommand += tableName;
                    else
                        throw new Exception("Missing Parameter. The specified Show Type requires a Table Name.");
                    break;
            }

            return (MySQLRecordset)this.OpenRecordset("SHOW " + varCommand);
        }
        #endregion

        public static byte getWeekMode()
        {
            if (DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek == DayOfWeek.Monday)
            {
                switch (DateTimeFormatInfo.CurrentInfo.CalendarWeekRule)
                {
                    case CalendarWeekRule.FirstDay:
                        return 7;
                    case CalendarWeekRule.FirstFourDayWeek:
                        return 3;
                    case CalendarWeekRule.FirstFullWeek:
                        return 3;
                }
            }
            else
            {
                switch (DateTimeFormatInfo.CurrentInfo.CalendarWeekRule)
                {
                    case CalendarWeekRule.FirstDay:
                        return 2;
                    case CalendarWeekRule.FirstFourDayWeek:
                        return 6;
                    case CalendarWeekRule.FirstFullWeek:
                        return 6;
                }
            }
            return 0;
        }
        public void OptimizeTable(string table)
        {
            this.Execute("OPTIMIZE NO_WRITE_TO_BINLOG TABLE " + table);
        }
        public void OptimizeAllTable()
        {
            foreach (string table in this.Tables)
            {
                this.OptimizeTable(table);
            }
        }
        public void LockTable(string tableName, MySQLLocktypes lockType)
        {
            if (tableName == null || tableName == "")
                throw new Exception("No Table specified.");

            string sqlstr = "";
            switch (lockType)
            {
                case MySQLLocktypes.MY_LOCK_READ:
                    sqlstr = " READ";
                    break;
                case MySQLLocktypes.MY_LOCK_READ_LOCAL:
                    sqlstr = " READ LOCAL";
                    break;
                case MySQLLocktypes.MY_LOCK_WRITE:
                    sqlstr = " WRITE";
                    break;
                case MySQLLocktypes.MY_LOCK_LOW_PRIORITY_WRITE:
                    sqlstr = " LOW_PRIORITY WRITE";
                    break;
            }

            this.Execute("LOCK TABLES " + tableName + sqlstr);
        }
        public void UnlockAllTables()
        {
            this.Execute("UNLOCK TABLES");
        }
        public void EnableTablesIndexes(params string[] tableNames)
        {
            if (tableNames == null || tableNames.Length == 0)
                return;
            else
            {
                foreach (string tableName in tableNames)
                {
                    this.Execute("ALTER TABLE `" + tableName + "` ENABLE KEYS");
                }
            }
        }
        public void DisableTablesIndexes(params string[] tableNames)
        {
            if (tableNames == null || tableNames.Length == 0)
                return;
            else
            {
                foreach (string tableName in tableNames)
                {
                    this.Execute("ALTER TABLE `" + tableName + "` DISABLE KEYS");
                }
            }
        }
        public void SetDateTimeLanguage(MySQLDateTimeLanguages language)
        {
            string languageCode = "";
            switch (language)
            {
                case MySQLDateTimeLanguages.English:
                    languageCode = "en_US";
                    break;
                case MySQLDateTimeLanguages.Dutch:
                    languageCode = "nl_BE";
                    break;
            }

            if (languageCode != "")
                this.Execute("SET lc_time_names = '" + languageCode + "';");
        }
        public static string getOrderNummerSQL(string fieldname)
        {
            return "CONCAT(  IF(SUBSTRING(" + fieldname + ",1,1)<>'0' AND CAST(" + fieldname + " AS UNSIGNED)=0," +
                               "LPAD('9',250,'9')," +
                               "LPAD(CAST(" + fieldname + " AS UNSIGNED),250,'0')" +
                              ")," +
                              "LPAD(" + fieldname + ",250,' ')" +
                           ") ";
        }
        /*
        public static string GetEnumToTextSubquery(string fieldname,
                                                   bool enumvalueAsText,
                                                   Type enumType,
                                                   string unknownText)
        {
            //CASE value WHEN [compare_value] THEN result [WHEN [compare_value] THEN result ...] [ELSE result] END 
            string varReturn = "";
            foreach (object enumValue in Enum.GetValues(enumType))
            {
                if (enumvalueAsText)
                    varReturn += "WHEN \"" + Strings.Chr(Convert.ToInt32(EnumValue)).ToString() + "\"";
                else
                    varReturn += "WHEN " + Convert.ToInt32(EnumValue).ToString();
                varReturn += " THEN \"" + StringTools.EnumNaamNaarString(Enum.GetName(enumType, EnumValue)) + "\" ";
            }
            varReturn = "(CASE " + fieldname + " " + varReturn + "ELSE \"" + unknownText + "\" END)";
            return varReturn;
        }
        */
        #endregion
    }
}
