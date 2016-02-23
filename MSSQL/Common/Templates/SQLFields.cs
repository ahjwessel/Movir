using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

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

        #region getInsertString/getUpdateString/getDeleteString
        internal protected virtual string getInsertString(string parTabel)
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

                    if (sbValues.Length > 0)
                        sbValues.Append(",");

                    sbFields.Append(fld.Name);
                    sbValues.Append(fld.SQLValue);
                }
            }

            return "INSERT INTO " + parTabel + " (" + sbFields.ToString() + ") VALUES (" + sbValues.ToString() + ")";
        }
        internal protected virtual string getUpdateString(string parTabel)
        {
            return getUpdateString(parTabel, this.getPrimaryKeys());
        }
        internal protected virtual string getUpdateString(string parTabel,string[] parPrimaryKeys)
        {
            var sbChangedFields = new StringBuilder();
            foreach (SQLField fld in this)
            {
                if (fld.IsDirty)
                {
                    if (sbChangedFields.Length > 0)
                        sbChangedFields.Append(",");

                    sbChangedFields.Append(fld.Name);
                    sbChangedFields.Append("=");
                    sbChangedFields.Append(fld.SQLValue);
                }
            }

            if (sbChangedFields.Length > 0)
            {
                var PrimaryKeysWhere = this.getPrimaryKeyWhere(parPrimaryKeys);
                if (PrimaryKeysWhere!=null)
                    return "UPDATE "+parTabel+ " SET " + sbChangedFields.ToString() + " WHERE " + PrimaryKeysWhere;
            }

            return "";
        }
        internal protected virtual string getDeleteString(string parTablename)
        {
            return getDeleteString(parTablename, this.getPrimaryKeys());
        }
        internal protected virtual string getDeleteString(string parTablename,string[] parPrimaryKeys)
        {
            var PrimaryKeysWhere = this.getPrimaryKeyWhere(parPrimaryKeys);
            if (PrimaryKeysWhere == null)
                return "";
            else
                return "DELETE * FROM " + parTablename + "WHERE " + PrimaryKeysWhere;
        }
        #endregion

        #region getPrimaryKeys
        public string[] getPrimaryKeys()
        {
            var Fieldsnames = new List<string>();
            foreach (SQLField fld in this)
            {
                if (fld.IsPrimaryKey)
                    Fieldsnames.Add(fld.Name);
            }

            if (Fieldsnames.Count == 0)
                return null;
            else
            {
                var mtxFieldsnames = new string[Fieldsnames.Count];
                Fieldsnames.CopyTo(mtxFieldsnames);
                Fieldsnames.Clear();
                return mtxFieldsnames;
            }
        }
        public string getPrimaryKeyWhere(params string[] parPrimaryKeys)
        {
            StringBuilder sbWhere = new StringBuilder();
            SQLField fld;
            foreach (var Fieldname in parPrimaryKeys)
            {
                fld = this[Fieldname];
                if (fld!=null)
                {
                    if (sbWhere.Length > 0)
                        sbWhere.Append(" AND ");

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
