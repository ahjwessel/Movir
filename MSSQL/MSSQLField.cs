using System;
using System.Data;
using Common.Templates;

namespace MSSQL
{
    public class MSSQLField:SQLField
    {
        public SqlDbType Type { get; protected set; }

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

        protected override string ConvertValueToSQL(object parValue)
        {
            return MSSQLConnector.ConvertValueToSQL(this.Type, parValue, this.AllowDBNull);
        }

        protected override object ConvertSQLToValue(object parSQLValue)
        {
            return MSSQLConnector.ConvertSQLToValue(this.Type, parSQLValue);
        }

        public MSSQLField(string parName, SqlDbType parType, object parInitValue)
            : this(parName, parType, parInitValue, false)
        { }
        public MSSQLField(string parName, SqlDbType parType, object parInitValue, bool parIsPrimaryKey)
            :this(parName,parType,parInitValue,parIsPrimaryKey,false,true)
        { }
        public MSSQLField(string parName, SqlDbType parType,object parInitValue,bool parIsPrimaryKey,bool parIsAutonumber, bool parAllowDBNull)
            :base(parName,parInitValue,parIsPrimaryKey,parIsAutonumber,parAllowDBNull)
        {
            this.Type = parType;
        }
        public MSSQLField(int parFieldIndex, DataTable parSchemaTable)
            :base(parFieldIndex,parSchemaTable)
        {
            this.Type = (SqlDbType)Enum.Parse(typeof(SqlDbType), parSchemaTable.Rows[parFieldIndex]["ProviderType"].ToString());
        }
    }
}
