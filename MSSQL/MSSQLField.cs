using System;
using System.Data;
using Common.Data;

namespace MSSQL
{
    public class MSSQLField:SQLField
    {
        public SqlDbType Type { get; private set; }

        public override string CreateLine
        {
            get
            {
                return this.Name + " " + 
                       this.Type.ToString() + " "+ 
                       (this.IsPrimaryKey ? "PRIMARY KEY " : "") + 
                       (this.AllowDBNull ? "" : "NOT NULL");
            }
        }

        protected override string ConvertValueToSQL(object value)
        {
            return MSSQLConnector.ConvertValueToSQL(this.Type, value, this.AllowDBNull);
        }

        protected override object ConvertSQLToValue(object SQLValue)
        {
            return MSSQLConnector.ConvertSQLToValue(this.Type, SQLValue);
        }

        public MSSQLField(string name, SqlDbType SQLType, object InitValue)
            : this(name, SQLType, InitValue, false)
        { }
        public MSSQLField(string name, SqlDbType SQLType, object InitValue, bool isPrimaryKey)
            :this(name,SQLType,InitValue,isPrimaryKey,false,true)
        { }
        public MSSQLField(string name, SqlDbType SQLType,object InitValue,bool isPrimaryKey,bool isAutonumber, bool allowDBNull)
            :base(name,InitValue,isPrimaryKey,isAutonumber,allowDBNull)
        {
            this.Type = SQLType;
        }
        public MSSQLField(int fieldIndex, DataTable schemaTable)
            :base(fieldIndex,schemaTable)
        {
            this.Type = (SqlDbType)Enum.Parse(typeof(SqlDbType), schemaTable.Rows[fieldIndex]["ProviderType"].ToString());
        }
    }
}
