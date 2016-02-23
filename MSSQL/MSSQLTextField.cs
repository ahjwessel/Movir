using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL
{
    public class MSSQLTextField:MSSQLField
    {
        public short MaxLength { get; protected set; }
        public override string CreateLine
        {
            get
            {
                return this.Name + " " + this.Type.ToString() + "(" + this.MaxLength + ") ";
            } 
        }
        public MSSQLTextField(string parName,short parMaxLength,string parInitValue)
            :base(parName,SqlDbType.VarChar,parInitValue, false,false,true)
        {
            this.MaxLength = parMaxLength;
        }
    }
}
