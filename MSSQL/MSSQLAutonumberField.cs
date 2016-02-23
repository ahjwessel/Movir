using System;
using System.Collections.Generic;
using System.Data;
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
                return this.Name + " " + this.Type.ToString()+ " IDENTITY(1,1) " + (this.IsPrimaryKey ? "PRIMARY KEY " : "");
            }
        }

        public MSSQLAutonumberField(string parName)
            :base(parName,SqlDbType.Int,0,true,true,false)
        { }
    }
}
