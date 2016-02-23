using System;
using System.Collections.Generic;
using Common.Templates;

namespace MSSQL
{
    public abstract class SQLTable:Table
    {
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
                if (parConnector.HasTable(tbl.Tablename))
                    parConnector.DeleteTable(tbl.Tablename);
                tbl.pCreateTable(parConnector, parIndexes);
            }
        }
        protected static List<SQLTable>  getAll(SQLConnector parConnector,
                                                Type parTableClass)
        {
            string varSQL;
            using ( var tbl = (SQLTable)System.Activator.CreateInstance(parTableClass))
            { 
                varSQL= tbl.getSQL();
            }
            return getAll(parConnector, parTableClass, varSQL);
        }
        protected static List<SQLTable> getAll(SQLConnector parConnector,
                                               Type parTableClass,
                                               string parSQL)
        {
            var Tables = new List<SQLTable>();
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

                            Tables.Add(tbl);
                            rec.MoveNext();
                        }
                    }
                    rec.Dispose();
                }
            }

            return Tables;
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
        protected virtual void pRead(SQLConnector parConnector, string parSQL)
        {
            using (var rec = parConnector.OpenRecordset(parSQL))
            {
                if (rec == null)
                {
                    this.RollbackToInitValues();
                    this.SetFlags(true, false);
                }
                else
                {
                    if (rec.EOF)
                    {
                        this.RollbackToInitValues();
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
        public virtual void Save(SQLConnector parConnector)
        {
            if (this.IsNew)
                parConnector.Execute(this.Fields.getInsertString(this.Tablename));
            else
                parConnector.Execute(this.Fields.getUpdateString(this.Tablename));
            this.SetFlags(false, true);
        }
        public virtual void Delete(SQLConnector parConnector)
        {
            parConnector.Execute(this.Fields.getDeleteString(this.Tablename));
            this.CreateNew();
        }
        #endregion

        public SQLTable(string parTablename, SQLFields parFields)
            :base(parTablename,parFields)
        { }
    }
}
