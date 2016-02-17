using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Templates
{
    public abstract class Fields : IDisposable, IEnumerable
    {
        protected Field[] _fields;
        public Field this[string parName]
        {
            get
            {
                Field varReturnValue = null;

                foreach (Field fld in this)
                {
                    if (fld.Name == parName)
                    {
                        varReturnValue = fld;
                        break;
                    }
                }
                if (varReturnValue == null)
                {
                    foreach (Field fld in this)
                    {
                        if (fld.Name.ToLower() == parName.ToLower())
                        {
                            varReturnValue = fld;
                            break;
                        }
                    }
                }

                return varReturnValue;
            }
        }
        public Field this[int parIndex]
        {
            get
            {
                return _fields[parIndex];
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
                bool varReturnValue = false;
                foreach (Field fld in this)
                {
                    if (fld.IsDirty)
                    {
                        varReturnValue = true;
                        break;
                    }
                }

                return varReturnValue;
            }
        }

        internal void SubmitValues()
        {
            foreach(Field fld in this)
            {
                fld.SubmitValue();
            }
        }
        internal void RollbackToOldValues()
        {
            foreach (Field fld in this)
            {
                fld.RollbackToOldValue();
            }
        }
        internal void RollbackToInitValues()
        {
            foreach (Field fld in this)
            {
                fld.RollbackToOldValue();
            }
        }

        public virtual void Dispose()
        {
            if (_fields!=null)
            {
                foreach(IDisposable dis in this)
                {
                    dis.Dispose();
                }
                _fields = null;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        public Fields(params Field[] parFields)
        {
            _fields = parFields;
        }
    }
}
