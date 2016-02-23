using System;
using System.IO;
using System.Linq;

namespace Common.Templates
{
    public abstract class Field : IDisposable
    {
        //Deze variabelen worden gebruikt om IsDirty te bepalen en 
        //Rollback.. en SubmitValues te regelen.
        //Dit is sneller omdat de properties wel eens overrided kunnen zijn
        object _InitValue = null;
        object _Value = null;
        object _OldValue = null;
        public string Name { get; private set; }
        public virtual object InitValue
        {
            get
            {
                return _InitValue;
            }
        }
        public virtual object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }
        public virtual object OldValue
        {
            get
            {
                return _OldValue;
            }
        }
        public bool IsDirty
        {
            get
            {
                if ((_Value == null && _OldValue != null) ||
                    (_Value != null && _OldValue == null))
                    return true;
                else if (_Value is byte[] && _OldValue is byte[])
                {
                    var BytesA = (byte[])_Value;
                    var BytesB = (byte[])_OldValue;

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
            _OldValue = GetCloneValue(_Value);
        }
        internal protected void RollbackToOldValue()
        {
            _Value = GetCloneValue(_OldValue);
        }
        internal protected void RollbackToInitValue()
        {
            _Value = GetCloneValue(_InitValue);
        }

        private static object GetCloneValue(object parValue)
        {
            if (parValue is ICloneable)
                return ((ICloneable)parValue).Clone();
            else
                return parValue;
        }

        public virtual void Dispose()
        {
            this.Name = null;
            _Value = null;
            _OldValue = null;
            _InitValue = null;
        }

        public Field(string parName,object parInitValue)
        {
            this.Name = parName;
            _InitValue = parInitValue;
            _Value = parInitValue;
            _OldValue = parInitValue;
        }
    }
}
