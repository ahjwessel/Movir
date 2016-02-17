using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Templates;
using System.Data;

namespace MSSQL
{
    public enum MSSQLFieldTypes
    {
        Bigint,
        Integer,
        Smallint,//Short
        Tinyint,//Byte
        Bit,
        Decimal,
        Money,
        Smallmoney,
        Float,
        Real,
        Datetime,
        Smalldatetime,
        Date,
        Time,
        Text,
        NText,//Unicode
        Binary,
        Image,
        uniqueidentifier//GUID
    }
    public class MSSQLField:SQLField
    {
        public MSSQLFieldTypes Type { get; protected set; }

        public override string CreateLine
        {
            get
            {
                var varName = this.Name;
                var varType = this.Type.ToString();

                var varIsNull = "";
                if (!this.AllowDBNull)
                    varIsNull = " NOT NULL";

                var varDefaultValue = " DEFAULT " + this.SQLInitValue;

                return varName + " " + varType + varIsNull + varDefaultValue;
            }
        }

        public MSSQLField(string parName, MSSQLFieldTypes parType,bool parAllowDBNull, object parInitValue)
            :base(parName,parAllowDBNull,parInitValue)
        {
            this.Type = parType;
        }
        public MSSQLField(int parFieldnumber, DataTable parSchemaTable)
            :base(parFieldnumber,parSchemaTable)
        {
            //Dit testen!!!!!!
            this.Type = (MSSQLFieldTypes)Enum.Parse(typeof(MSSQLFieldTypes), parSchemaTable.Rows[parFieldnumber - 1]["ProviderType"].ToString());
        }

        protected override string ConvertValueToSQL(object parValue)
        {
            throw new NotImplementedException();
        }

        protected override object ConvertSQLToValue(string parSQLValue)
        {
            throw new NotImplementedException();
        }

        public override void Refresh(DataRow parRow)
        {
            throw new NotImplementedException();
        }
    }
}
