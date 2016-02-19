using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Templates
{
    public abstract class Field : IDisposable
    {
        public string Name { get; private set; }
        public object InitValue { get; private set; }
        public object Value { get; set; }
        public object OldValue { get; set; }
        public bool IsDirty
        {
            get
            {
                if ((this.Value == null && this.OldValue != null) ||
                    (this.Value != null && this.OldValue == null))
                    return true;
                else if (this.Value is MemoryStream && this.OldValue is MemoryStream)
                {
                    var BytesA = ((MemoryStream)this.Value).GetBuffer();
                    var BytesB = ((MemoryStream)this.OldValue).GetBuffer();

                    if (BytesA.LongLength != BytesB.LongLength)
                        return true;
                    else if (!BytesA.SequenceEqual(BytesB))
                        return true;
                    else
                        return false;
                }
                else
                    return !this.Value.Equals(this.OldValue);
            }
        }

        internal protected void SubmitValue()
        {
            this.OldValue = GetCloneValue(this.Value);
        }
        internal protected void RollbackToOldValue()
        {
            this.Value = GetCloneValue(this.OldValue);
        }
        internal protected void RollbackToInitValue()
        {
            this.Value = GetCloneValue(this.InitValue);
        }

        private static object GetCloneValue(object parValue)
        {
            if (parValue is ICloneable)
                return ((ICloneable)parValue).Clone();
            else if (parValue is MemoryStream)
                return new MemoryStream(((MemoryStream)parValue).ToArray());
            else
                return parValue;
        }

        public virtual void Dispose()
        {
            this.Name = null;
            this.Value = null;
            this.OldValue = null;
            this.InitValue = null;
        }

        public Field(string parName,object parInitValue)
        {
            this.Name = parName;
            this.InitValue = parInitValue;
            this.OldValue = parInitValue;
            this.Value = parInitValue;
        }
    }
}
