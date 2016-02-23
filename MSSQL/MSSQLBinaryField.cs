using System.Data;
using System.IO;

namespace MSSQL
{
    public class MSSQLBinaryField:MSSQLField
    {
        public override object InitValue
        {
            get
            {
                if (base.InitValue == null)
                    return null;
                else 
                    return new MemoryStream((byte[])base.InitValue);
            }
        }
        public override object Value
        {
            get
            {
                if (base.Value == null)
                    return null;
                else
                    return new MemoryStream((byte[])base.Value);
            }

            set
            {
                if (value == null)
                    base.Value = null;
                else if (value is MemoryStream)
                    base.Value = ((MemoryStream)value).ToArray();
                else
                    base.Value = value;
            }
        }
        public override object OldValue
        {
            get
            {
                if (base.OldValue == null)
                    return null;
                else
                    return new MemoryStream((byte[])base.OldValue);
            }
        }
        public override string CreateLine
        {
            get
            {
                return this.Name + " " + this.Type.ToString() + "(max) " + 
                       (this.AllowDBNull ? "" : "NOT NULL") ;
            }
        }
        public MSSQLBinaryField(string parName)
            : this(parName, SqlDbType.VarBinary)
        { }
        protected MSSQLBinaryField(string parName, SqlDbType parType)
            : base(parName, parType, null)
        { }
    }
}
