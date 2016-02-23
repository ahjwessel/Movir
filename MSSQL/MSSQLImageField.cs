using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL
{
    public class MSSQLImageField:MSSQLField
    {
        public MSSQLImageField(string parName)
            : base(parName, System.Data.SqlDbType.Image, null)
        { }
    }
}
