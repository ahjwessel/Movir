using System;
using System.Data;
using MySql.Data.MySqlClient;
using Common.Data;

namespace MySQL
{
    public class MySQLField:SQLField
    {
        public MySqlDbType Type { get; private set; }

        public override string CreateLine
        {
            get
            {
                /*
                string ColumnType;
                switch (this.Type)
                {
                    case MySqlDbType.Binary:

                }
                */
                return this.Name + " " +
                       this.Type.ToString() + " " +
                       (this.IsPrimaryKey ? "PRIMARY KEY " : "") +
                       (this.AllowDBNull ? "" : "NOT NULL");
            }
        }

        protected override string ConvertValueToSQL(object value)
        {
            return MySQLConnector.ConvertValueToSQL(this.Type, value, this.AllowDBNull);
        }

        protected override object ConvertSQLToValue(object SQLValue)
        {
            return MySQLConnector.ConvertSQLToValue(this.Type, SQLValue);
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
