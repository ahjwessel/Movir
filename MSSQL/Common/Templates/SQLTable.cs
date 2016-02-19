using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Templates;

namespace MSSQL
{
    public abstract class SQLTable:Table
    {
        protected string[] PrimaryKeys { get; private set; }
        new public SQLFields Fields
        {
            get
            {
                return (SQLFields)base.Fields;
            }
        }

        #region Statics
        protected static void CreateTable(SQLConnector parConnector,
                                          Type parTableClass,
                                          params SQLIndex[] parIndexes)
        {
            using (var tbl = (SQLTable)System.Activator.CreateInstance(parTableClass))
            {
                tbl.pCreateTable(parConnector, parIndexes);
            }
        }
        protected static ArrayList getAll(SQLConnector parConnector,
                                          Type parTableClass)
        {
            string varSQL;
            using ( var tbl = (SQLTable)System.Activator.CreateInstance(parTableClass))
            { 
                varSQL= tbl.getSQL();
            }
            return getAll(parConnector, parTableClass, varSQL);
        }
        protected static ArrayList getAll(SQLConnector parConnector,
                                          Type parTableClass,
                                          string parSQL)
        {
            var arrReturn = new ArrayList();
            using (var rec = parConnector.OpenRecordset(parSQL))
            {
                if (rec != null)
                {
                    if (!rec.EOF)
                    {
                        SQLTable tbl = null;
                        rec.MoveFirst();
                        while (!rec.EOF)
                        {
                            tbl = (SQLTable)System.Activator.CreateInstance(parTableClass);
                            tbl.RecToTyp(rec);

                            arrReturn.Add(tbl);
                            rec.MoveNext();
                        }
                    }
                    rec.Dispose();
                }
            }

            return arrReturn;
        }
        private static SQLField getField(SQLRecordset parRecordset,
                                params string[] parPossibleFieldnames)
        {
            SQLField fld = null;
            foreach (string varFieldname in parPossibleFieldnames)
            {
                fld = parRecordset.Fields[varFieldname];
                if (fld != null)
                    break;
            }

            return fld;
        }
        #endregion

        #region Protected
        protected abstract void pCreateTable(SQLConnector parConnector,params SQLIndex[] parIndexes);
        protected virtual void RecToTyp(SQLRecordset parRecordset)
        {
            SQLField recField = null;
            
            foreach (SQLField tblField in this.Fields)
            {
                recField = getField(parRecordset, tblField.Name, this.Tablename + "." + tblField.Name);
                if (recField != null)
                    tblField.Value = recField.Value;
            }
            this.SetFlags(false, true);
            this.SubmitValues();
        }
        protected virtual void TypToRec(SQLRecordset parRecordset)
        {
            SQLField recField = null;

            foreach (SQLField tblField in this.Fields)
            {
                recField = getField(parRecordset, tblField.Name, this.Tablename + "." + tblField.Name);
                if (recField != null)
                    recField.Value= tblField.Value;
            }
            this.SetFlags(false, true);
            this.SubmitValues();
        }
        protected virtual string getSQL()
        {
            return "SELECT * FROM " + this.Tablename + " ";
        }
        protected virtual string getPrimaryKeyWhere()
        {
            var ReturnValue = new StringBuilder();
            SQLField Field;
            foreach (var PrimaryKeyName in this.PrimaryKeys)
            {
                if (ReturnValue.Length > 0)
                    ReturnValue.Append(" AND ");

                Field = this.Fields[PrimaryKeyName];
                ReturnValue.Append(Field.Name + "=" + Field.SQLOldValue);
            }

            return ReturnValue.ToString();
        }
        protected virtual void pRead(SQLConnector parConnector, string parSQL)
        {
            using (var rec = parConnector.OpenRecordset(parSQL))
            {
                if (rec == null)
                {
                    this.EmptyValues();
                    this.SetFlags(true, false);
                }
                else
                {
                    if (rec.EOF)
                    {
                        this.EmptyValues();
                        this.SetFlags(true, false);
                    }
                    else
                    {
                        rec.MoveFirst();
                        this.RecToTyp(rec);
                        this.SetFlags(false, true);
                    }
                }
            }
        }
        protected virtual void pSave(SQLConnector parConnector,
                                     string parNewSQL,
                                     string parExistingSQL)
        {
            if (this.IsNew)
            {
                using (var rec = parConnector.OpenRecordset(parNewSQL))
                {
                    if (rec != null)
                    {
                        rec.AddNew();
                        TypToRec(rec);
                        rec.Update();

                        this.SetFlags(false, true);
                    }
                }
            }
            else
            {
                using (var rec = parConnector.OpenRecordset(parExistingSQL))
                {
                    if (rec != null)
                    {
                        if (rec.EOF)
                        {
                            this.SetFlags(true, true);
                            this.pSave(parConnector, parNewSQL, parExistingSQL);
                            return;
                        }
                        else
                        {
                            rec.Edit();
                            TypToRec(rec);
                            rec.Update();

                            this.SetFlags(false, true);
                        }
                    }
                }
            }
        }
        protected virtual void pDelete(SQLConnector parConnection, string parSQL)
        {
            using (var rec = parConnection.OpenRecordset(parSQL))
            {
                if (rec != null)
                {
                    if (!rec.EOF)
                    {
                        rec.MoveFirst();
                        rec.Delete();
                    }
                    this.CreateNew();
                }
            }
        }
        #endregion

        public override void Dispose()
        {
            this.PrimaryKeys = null;
            base.Dispose();
        }

        public SQLTable(string parTablename, string parPrimaryKey, SQLFields parFields)
            :this(parTablename,new string[] { parPrimaryKey },parFields)
        { }
        public SQLTable(string parTablename, string[] parPrimaryKeys, SQLFields parFields)
            : base(parTablename, parFields)
        {
            this.PrimaryKeys = parPrimaryKeys;
        }
    }
}
