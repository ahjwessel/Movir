using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL
{
    public class MSSQLDecimalField:MSSQLField
    {
        public byte Precision { get; private set; }
        public byte Scale { get; private set; }
        public override string CreateLine
        {
            get
            {
                return this.Name + " " +
                       this.Type.ToString() +
                       "(" + this.Precision.ToString() + "," + this.Scale.ToString() + ")";
            }
        }
        public MSSQLDecimalField(string name,byte precision,byte scale, decimal InitValue)
            :base(name,SqlDbType.Decimal,InitValue,false,false,false)
        {
            this.Precision = precision;
            this.Scale = scale;
        }
    }
}
