using System.Data;

namespace MSSQL
{
    public class MSSQLTextField:MSSQLField
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
        public MSSQLTextField(string name,short parMaxLength,bool parUnicode,string InitValue)
            :base(name,(parUnicode ? SqlDbType.NVarChar : SqlDbType.VarChar), InitValue, false,false,true)
        {
            if (parMaxLength < 0)
                this.MaxLength = 0;
            else if (parUnicode && parMaxLength > 4000)
                this.MaxLength = 0;//Gelijk aan max
            else if (!parUnicode && parMaxLength > 8000)
                this.MaxLength = 0;//Gelijk aan max
            else
                this.MaxLength = parMaxLength;
        }
    }
}
