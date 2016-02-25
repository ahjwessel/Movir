using System;
using System.Collections.Generic;
using Common.Data;

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
        protected static void CreateTable(SQLConnector connector,
                                          Type tableClass,
                                          params SQLIndex[] indexes)
        {
            using (var tbl = (SQLTable)System.Activator.CreateInstance(tableClass))
            {
                if (connector.HasTable(tbl.Tablename))
                    connector.DeleteTable(tbl.Tablename);
                tbl.pCreateTable(connector, indexes);
            }
        }
        protected static List<SQLTable>  getAll(SQLConnector connector,
                                                Type tableClass)
        {
            string varSQL;
            using ( var tbl = (SQLTable)System.Activator.CreateInstance(tableClass))
            { 
                varSQL= tbl.GetSQL();
            }
            return GetAll(connector, tableClass, varSQL);
        }
        protected static List<SQLTable> GetAll(SQLConnector connector,
                                               Type tableClass,
                                               string SQL)
        {
            var Tables = new List<SQLTable>();
            using (var rec = connector.OpenRecordset(SQL))
            {
                if (rec != null)
                {
                    if (!rec.EOF)
                    {
                        SQLTable tbl = null;
                        rec.MoveFirst();
                        while (!rec.EOF)
                        {
                            tbl = (SQLTable)System.Activator.CreateInstance(tableClass);
                            tbl.RecToTyp(rec);

                            Tables.Add(tbl);
                            rec.MoveNext();
                        }
                    }
                }
            }

            return Tables;
        }
        private static SQLField GetField(SQLRecordset recordset,
                                         params string[] possibleFieldnames)
        {
            SQLField field = null;
            foreach (string fieldname in possibleFieldnames)
            {
                field = recordset.Fields[fieldname];
                if (field != null)
                    break;
            }

            return field;
        }
        #endregion

        #region Protected
        protected abstract void pCreateTable(SQLConnector connector,params SQLIndex[] indexes);
        protected virtual void RecToTyp(SQLRecordset recordset)
        {
            SQLField recordField = null;
            
            foreach (SQLField tableField in this.Fields)
            {
                recordField = GetField(recordset, tableField.Name, this.Tablename + "." + tableField.Name);
                if (recordField != null)
                    tableField.Value = recordField.Value;
            }
            this.SetFlags(false, true);
            this.SubmitValues();
        }
        protected virtual void TypToRec(SQLRecordset recordset)
        {
            SQLField recordField = null;

            foreach (SQLField tableField in this.Fields)
            {
                recordField = GetField(recordset, tableField.Name, this.Tablename + "." + tableField.Name);
                if (recordField != null)
                    recordField.Value= tableField.Value;
            }
            this.SetFlags(false, true);
            this.SubmitValues();
        }
        protected virtual string GetSQL()
        {
            return "SELECT * FROM " + this.Tablename + " ";
        }
        protected virtual void pRead(SQLConnector connector, string SQL)
        {
            using (var rec = connector.OpenRecordset(SQL))
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
        public virtual void Save(SQLConnector connector)
        {
            if (this.IsNew)
                connector.Execute(this.Fields.GetInsertString(this.Tablename));
            else
                connector.Execute(this.Fields.GetUpdateString(this.Tablename));
            this.SetFlags(false, true);
        }
        public virtual void Delete(SQLConnector connector)
        {
            connector.Execute(this.Fields.GetDeleteString(this.Tablename));
            this.CreateNew();
        }
        #endregion

        public SQLTable(string tablename, SQLFields parFields)
            :base(tablename,parFields)
        { }
    }
}
