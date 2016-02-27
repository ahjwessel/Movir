using MySql.Data.MySqlClient;
using System.Data;

namespace MySQL
{
    public class MySQLTextField:MySQLField
    {
        public short MaxLength { get; protected set; }
        public override object InitValue
        {
            get
            {
                return (string)base.InitValue;
            }
        }
        public override object Value
        {
            get
            {
                return (string)base.Value;
            }
            set
            {
                base.Value = (string)value;
            }
        }
        public override object OldValue
        {
            get
            {
                return (string)base.OldValue;
            }
        }
        public override string CreateLine
        {
            get
            {
                return this.Name + " " + 
                       this.Type.ToString() + 
                       "(" + (this.MaxLength==0 ? "max" : this.MaxLength.ToString())  + ") ";
            } 
        }
        public MySQLTextField(string name,short parMaxLength,string InitValue)
            :base(name, MySqlDbType.Text, InitValue, false,false,true)
        {
            if (parMaxLength < 0)
                this.MaxLength = 0;
            else
                this.MaxLength = parMaxLength;
        }
    }
}
