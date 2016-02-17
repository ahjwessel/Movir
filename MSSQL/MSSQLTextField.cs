using System;
using System.Collections.Generic;
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
                return this.Name + " " + this.Type.ToString() + "(" + this.MaxLength + ")" + "DEFAULT \"" + this.SQLInitValue + "\"";
            } 
        }
        public MSSQLTextField(string parName,short parMaxLength,bool parUnicode,string parInitValue)
            :base(parName,parUnicode ? MSSQLFieldTypes.NText : MSSQLFieldTypes.Text,true,parInitValue)
        { }
    }
}
