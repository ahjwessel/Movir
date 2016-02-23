using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL
{
    public class MSSQLBinaryField:MSSQLField
    {
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

                return varName + " " + varType + "(max)" + varPrimary + varIsNull;
            }
        }
        public MSSQLBinaryField(string parNaam)
            : base(parNaam, System.Data.SqlDbType.VarBinary, null)
        { }
    }
}
