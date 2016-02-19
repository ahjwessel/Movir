using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Templates
{
    public abstract class SQLFields:Fields
    {
        new public SQLField this[string parName]
        {
            get
            {
                return (SQLField)base[parName];
            }
        }
        new public SQLField this[int parIndex]
        {
            get
            {
                return (SQLField)base[parIndex];
            }
        }

        #region getAddString/getUpdateString/getFieldsString/getValuesString
        internal protected string getInsertString()
        {
            StringBuilder sbFields = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();

            foreach (SQLField fld in this)
            {
                if (fld.Value != null &&
                    !(fld.IsAutonumber && Convert.ToInt32(fld.Value) >= 0))
                {
                    if (sbFields.Length > 0)
                        sbFields.Append(",");
                    sbFields.Append(fld.Name);

                    if (sbValues.Length > 0)
                        sbValues.Append(",");
                      sbValues.Append(fld.SQLValue);
                }
            }

            return "(" + sbFields.ToString() + ") VALUES (" + sbValues.ToString() + ")";
        }
        internal string getUpdateString()
        {
            StringBuilder sbString = new StringBuilder();
            foreach (SQLField fld in this)
            {
                if (fld.IsDirty)
                {
                    if (sbString.Length > 0)
                    {
                        sbString.Append(",");
                    }

                    sbString.Append(fld.Name + "=");
                    sbString.Append(fld.SQLValue);
                }
            }

            if (sbString.Length > 0)
                return "SET " + sbString.ToString() + " WHERE " + this.getPrimaryKeyWhere();
            else
                return "";
        }
        #endregion

        #region getPrimaryKeyWhere
        internal string getPrimaryKeyWhere()
        {
            StringBuilder sbWhere = new StringBuilder();
            foreach (SQLField fld in this)
            {
                if (fld.IsPrimaryKey)
                {
                    if (sbWhere.Length > 0)
                    {
                        sbWhere.Append(" AND ");
                    }
                    sbWhere.Append(fld.Name + "=" + fld.SQLValue);
                }
            }

            return sbWhere.ToString();
        }
        #endregion

        #region Refresh/ClearValues
        internal protected void Refresh(DataRow parRow)
        {
            foreach (SQLField fld in this)
            {
                fld.Refresh(parRow);
            }
        }
        #endregion

        public SQLFields(params SQLField[] parFields)
            :base(parFields)
        { }
    }
}
