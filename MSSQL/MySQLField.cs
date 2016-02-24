using System;
using Common.Data;
using System.Data;
using MySql.Data.MySqlClient;

namespace MSSQL
{
    public class MySQLField:SQLField
    {
        public MySqlDbType Type { get; private set; }

        protected override string ConvertValueToSQL(object value)
        {
            throw new NotImplementedException();
        }

        protected override object ConvertSQLToValue(object SQLValue)
        {
            throw new NotImplementedException();
        }
        public override string CreateLine
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public MySQLField(string name, MySqlDbType SQLType, object InitValue)
            : this(name, SQLType, InitValue, false)
        { }
        public MySQLField(string name, MySqlDbType SQLType, object InitValue, bool isPrimaryKey)
            :this(name,SQLType,InitValue,isPrimaryKey,false,true)
        { }
        public MySQLField(string name, MySqlDbType SQLType, object InitValue, bool isPrimaryKey, bool isAutonumber, bool allowDBNull)
            :base(name,InitValue,isPrimaryKey,isAutonumber,allowDBNull)
        {
            this.Type = SQLType;
        }
        public MySQLField(int fieldIndex, DataTable schemaTable)
            :base(fieldIndex,schemaTable)
        {
            this.Type = (MySqlDbType)Enum.Parse(typeof(MySqlDbType), schemaTable.Rows[fieldIndex]["ProviderType"].ToString());
        }
    }
}
