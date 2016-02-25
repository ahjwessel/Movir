using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Common.Data
{
    public abstract class SQLFields:Fields
    {
        new public SQLField this[string name]
        {
            get
            {
                return (SQLField)base[name];
            }
        }
        new public SQLField this[int index]
        {
            get
            {
                return (SQLField)base[index];
            }
        }

        #region GetInsertString/getUpdateString/getDeleteString
        internal protected virtual string GetInsertString(string tabel)
        {
            StringBuilder fields = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (SQLField field in this)
            {
                if (field.Value != null &&
                    !(field.IsAutonumber && Convert.ToInt32(field.Value) >= 0))
                {
                    if (fields.Length > 0)
                        fields.Append(",");

                    if (values.Length > 0)
                        values.Append(",");

                    fields.Append(field.Name);
                    values.Append(field.SQLValue);
                }
            }

            return "INSERT INTO " + tabel + " (" + fields.ToString() + ") VALUES (" + values.ToString() + ")";
        }
        internal protected virtual string GetUpdateString(string tabel)
        {
            return GetUpdateString(tabel, this.GetPrimaryKeys());
        }
        internal protected virtual string GetUpdateString(string tabel,string[] primaryKeys)
        {
            var changedFields = new StringBuilder();
            foreach (SQLField field in this)
            {
                if (field.IsDirty)
                {
                    if (changedFields.Length > 0)
                        changedFields.Append(",");

                    changedFields.Append(field.Name);
                    changedFields.Append("=");
                    changedFields.Append(field.SQLValue);
                }
            }

            if (changedFields.Length > 0)
            {
                var PrimaryKeysWhere = this.GetPrimaryKeyWhere(primaryKeys);
                if (PrimaryKeysWhere!=null)
                    return "UPDATE "+tabel+ " SET " + changedFields.ToString() + " WHERE " + PrimaryKeysWhere;
            }

            return "";
        }
        internal protected virtual string GetDeleteString(string tablename)
        {
            return GetDeleteString(tablename, this.GetPrimaryKeys());
        }
        internal protected virtual string GetDeleteString(string tablename,string[] primaryKeys)
        {
            var primaryKeysWhere = this.GetPrimaryKeyWhere(primaryKeys);
            if (primaryKeysWhere == null)
                return "";
            else
                return "DELETE * FROM " + tablename + "WHERE " + primaryKeysWhere;
        }
        #endregion

        #region GetPrimaryKeys
        public string[] GetPrimaryKeys()
        {
            var fieldsnames = new List<string>();
            foreach (SQLField field in this)
            {
                if (field.IsPrimaryKey)
                    fieldsnames.Add(field.Name);
            }

            if (fieldsnames.Count == 0)
                return null;
            else
            {
                var mtxFieldsnames = new string[fieldsnames.Count];
                fieldsnames.CopyTo(mtxFieldsnames);
                fieldsnames.Clear();
                return mtxFieldsnames;
            }
        }
        public string GetPrimaryKeyWhere(params string[] primaryKeys)
        {
            StringBuilder where = new StringBuilder();
            SQLField field;
            foreach (var Fieldname in primaryKeys)
            {
                field = this[Fieldname];
                if (field!=null)
                {
                    if (where.Length > 0)
                        where.Append(" AND ");

                    where.Append(field.Name + "=" + field.SQLValue);
                }
            }

            return where.ToString();
        }
        #endregion

        #region Refresh/ClearValues
        internal protected void Refresh(DataRow row)
        {
            foreach (SQLField field in this)
            {
                field.Refresh(row);
            }
        }
        #endregion

        public SQLFields(SQLField[] parFields)
            :base(parFields)
        { }
        protected SQLFields()
            : base()
        { }
    }
}
