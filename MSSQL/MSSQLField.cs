using System;
using System.Data;
using Common.Templates;

namespace MSSQL
{
    public class MSSQLField:SQLField
    {
        public MSSQLFieldTypes Type { get; protected set; }

        public override string CreateLine
        {
            get
            {
                var varName = this.Name;
                var varType = this.Type.ToString();
                var varPrimary = this.IsPrimaryKey ? "PRIMARY KEY " : "";

                var varIsNull = "";
                if (!this.AllowDBNull)
                    varIsNull = " NOT NULL";

                var varDefaultValue = " DEFAULT " + this.SQLInitValue;

                return varName + " " + varType + varPrimary + varIsNull + varDefaultValue;
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

        public MSSQLField(string parName, MSSQLFieldTypes parType,bool parAllowDBNull, object parInitValue)
            :base(parName,parAllowDBNull,parInitValue)
        {
            this.Type = parType;
        }
        public MSSQLField(int parFieldnumber, DataTable parSchemaTable)
            :base(parFieldnumber,parSchemaTable)
        {
            this.Type = (MSSQLFieldTypes)Enum.Parse(typeof(MSSQLFieldTypes), parSchemaTable.Rows[parFieldnumber - 1]["ProviderType"].ToString());
        }
    }
}
