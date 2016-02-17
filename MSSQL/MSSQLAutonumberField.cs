using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL
{
    public class MSSQLAutonumberField:MSSQLField
    {
        public override string CreateLine
        {
            get
            {
                return this.Name + " " + this.Type.ToString()+ " AUTO_INCREMENT";
            }
        }

        public MSSQLAutonumberField(string parName)
            :base(parName,MSSQLFieldTypes.Integer,false,0)
        { }
    }
}
