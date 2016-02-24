using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MSSQL
{
    public class MSSQLImageField:MSSQLBinaryField
    {
        public override object InitValue
        {
            get
            {
                var tmp = base.InitValue;
                if (tmp == null)
                    return null;
                else
                    return new Bitmap((MemoryStream)tmp);
            }
        }
        public override object Value
        {
            get
            {
                object tmp = base.Value;
                if (tmp == null)
                    return null;
                else
                    return new Bitmap((MemoryStream)tmp);
            }
            set
            {
                if (value == null)
                    base.Value = null;
                else if (value is Image)
                {
                    using (var mem = new MemoryStream())
                    {
                        ((Image)value).Save(mem, ImageFormat.Png);
                        base.Value = mem;
                    }
                }
                else
                    base.Value = value;
            }
        }
        public override object OldValue
        {
            get
            {
                object tmp = base.OldValue;
                if (tmp == null)
                    return null;
                else
                    return new Bitmap((MemoryStream)tmp);
            }
        }
        public override string CreateLine
        {
            get
            {
                return this.Name + " " + this.Type.ToString() + " " +
                       (this.AllowDBNull ? "" : "NOT NULL");
            }
        }

        public MSSQLImageField(string name)
            : base(name, System.Data.SqlDbType.Image)
        { }
    }
}
