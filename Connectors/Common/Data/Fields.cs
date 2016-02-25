using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Common.Data
{
    public abstract class Fields : IDisposable, IEnumerable<Field>
    {
        protected Field[] _fields;
        public Field this[string name]
        {
            get
            {
                Field returnValue = (from field in this where field.Name==name select field).FirstOrDefault();
                if (default(Field)==returnValue)
                {
                    returnValue = (from field in this where field.Name.ToLower() == name.ToLower() select field).FirstOrDefault();
                    if (default(Field) == returnValue)
                        returnValue = null;
                }
                return returnValue;
            }
        }
        public Field this[int index]
        {
            get
            {
                return _fields[index];
            }
        }
        public int Count
        {
            get
            {
                return _fields.Length;
            }
        }
        public bool IsDirty
        {
            get
            {
                foreach (Field field in this)
                {
                    if (field.IsDirty)
                        return true;
                }

                return false;
            }
        }

        public object[] GetValues()
        {
            var values = new object[this.Count];
            for (int counter=0;counter<this.Count;counter++)
            {
                values[counter] = _fields[counter].Value;
            }
            return values;
        }

        internal protected void SubmitValues()
        {
            foreach(Field field in this)
            {
                field.SubmitValue();
            }
        }
        internal protected void RollbackToOldValues()
        {
            foreach (Field field in this)
            {
                field.RollbackToOldValue();
            }
        }
        internal protected void RollbackToInitValues()
        {
            foreach (Field field in this)
            {
                field.RollbackToOldValue();
            }
        }

        public virtual void Dispose()
        {
            foreach(IDisposable dis in this)
            {
                dis.Dispose();
            }
        }

        public IEnumerator<Field> GetEnumerator()
        {
            return ((IEnumerable<Field>)_fields).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Field>)_fields).GetEnumerator();
        }

        public Fields(Field[] parFields)
        {
            _fields = parFields;
        }
        protected Fields()
        { }
    }
}
