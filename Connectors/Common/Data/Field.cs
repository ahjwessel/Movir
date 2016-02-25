using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace Common.Data
{
    public abstract class Field : IDisposable
    {
        //Deze variabelen worden gebruikt om IsDirty te bepalen en 
        //Rollback.. en SubmitValues te regelen.
        //Dit is sneller omdat de properties wel eens overrided kunnen zijn
        object _initValue = null;
        object _value = null;
        object _oldValue = null;
        public string Name { get; private set; }
        public virtual object InitValue
        {
            get
            {
                return _initValue;
            }
        }
        public virtual object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
        public virtual object OldValue
        {
            get
            {
                return _oldValue;
            }
        }
        public bool IsDirty
        {
            get
            {
                if ((_value == null && _oldValue != null) ||
                    (_value != null && _oldValue == null))
                    return true;
                else if (_value is IList && _oldValue is IList)
                {
                    var listA = (IList)_value;
                    var listB = (IList)_oldValue;

                    if (listA.Count == listB.Count)
                        return true;
                    else
                    {
                        for (int counter=0;counter<listA.Count;counter++)
                        {
                            if (!listA[counter].Equals(listB[counter]))
                                return true;
                        }
                        return false;
                    }
                }
                else
                    return !_value.Equals(_oldValue);
            }
        }

        internal protected void SubmitValue()
        {
            _oldValue = GetCloneValue(_value);
        }
        internal protected void RollbackToOldValue()
        {
            _value = GetCloneValue(_oldValue);
        }
        internal protected void RollbackToInitValue()
        {
            _value = GetCloneValue(_initValue);
        }

        private static object GetCloneValue(object Value)
        {
            if (Value is ICloneable)
                return ((ICloneable)Value).Clone();
            else
                return Value;
        }

        public virtual void Dispose()
        {
            this.Name = null;
            _value = null;
            _oldValue = null;
            _initValue = null;
        }

        public Field(string name,object InitValue)
        {
            this.Name = name;
            _initValue = InitValue;
            _value = InitValue;
            _oldValue = InitValue;
        }
    }
}
