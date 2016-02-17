using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace Common.Templates
{
    public enum enmRecordsetState : byte
    {
        Viewing = 0,
        Appending = 1,
        Editing = 2,
        Deleted = 3
    }

    public abstract class SQLRecordset:IDisposable
    {
        public SQLConnection Connection { get; protected set; }
        public DataTable SchemaTable{get; protected set;}
        public DataSet DataSet { get; protected set; }
        public string Tablename { get; protected set; }
        public string SQL { get; protected set; }
        public int RecordCount { get; protected set; }
        public int CurrentRecordNumber{get; protected set;}
        protected SQLRecord CurrentRecord { get; set; }
        protected ArrayList RecordsDeleted { get; set; }
        public enmRecordsetState State { get; protected set; }

        public int AbsolutePosition
        {
            get
            {
                return this.CurrentRecordNumber - 1;
            }
        }
        public bool BOF
        {
            get
            {
                return (this.RecordCount == 0 || this.CurrentRecordNumber < 1);
            }
        }
        public bool EOF
        {
            get
            {
                return (this.RecordCount == 0 || this.CurrentRecordNumber > this.RecordCount);
            }
        }
        public SQLFields Fields
        {
            get
            {
                return this.CurrentRecord.Fields;
            }
        }

        #region OpenRecordset/CloseRecordset
        public bool OpenRecordset(string parSQL)
        {
            this.SQL = parSQL;
            return this.Requery();
        }
        public bool Requery()
        {
            this.CloseRecordset(false);

            Exception exThrow = null;
            var cmd = this.Connection.CreateCommand(this.SQL);
            cmd.CommandTimeout = this.Connection.TimeoutOpenRecordsetInSeconden;
            DbDataReader rdr = cmd.ExecuteReader();
            try
            {
                this.SchemaTable = rdr.GetSchemaTable();
                this.DataSet = new DataSet();
                this.DataSet.EnforceConstraints = false;
                this.DataSet.Load(rdr, LoadOption.OverwriteChanges, "tbl");
                this.RecordCount = this.DataSet.Tables[0].Rows.Count;
                this.Tablename = this.SchemaTable.Rows[0]["BaseTableName"].ToString();
                this.CurrentRecord = this.CreateRecord(this.SchemaTable, this.DataSet);
                if (this.RecordCount > 0)
                {
                    this.MoveFirst();
                }
            }
            catch (Exception ex)
            {
                exThrow = ex;
            }

            rdr.Close();
            rdr.Dispose();
            cmd.Dispose();

            if (exThrow != null)
                throw exThrow;

            return true;
        }
        public void CloseRecordset()
        {
            this.CloseRecordset(true);
        }
        private void CloseRecordset(bool parResetSQL)
        {
            if (this.SchemaTable != null)
            {
                this.SchemaTable.Clear();
                this.SchemaTable.Dispose();
                this.SchemaTable = null;
            }
            if (this.DataSet != null)
            {
                this.DataSet.Clear();
                this.DataSet.Dispose();
                this.DataSet = null;
            }
            this.RecordCount = 0;
            this.CurrentRecordNumber = 0;
            this.DisposeCurrentRecord();
            this.DisposeDeletedRecords();

            if (parResetSQL)
                this.SQL = "";
        }
        protected abstract SQLRecord CreateRecord(DataTable parSchemaTable, DataSet parDataSet);
        #endregion

        private void ReadCurrentRecord()
        {
            if (this.DataSet != null && this.CurrentRecord != null &&
                this.CurrentRecordNumber > 0 && this.CurrentRecordNumber <= this.RecordCount &&
                (this.RecordsDeleted == null || !this.RecordsDeleted.Contains(this.CurrentRecordNumber)))
                this.CurrentRecord.Refresh(this.DataSet.Tables[0].Rows[this.CurrentRecordNumber - 1]);
            else
            {
                this.CurrentRecord.ClearValues();
                this.State = enmRecordsetState.Deleted;
            }
        }

        #region Move... voids
        public void MoveToRecord(int parRecordnumber)
        {
            this.CurrentRecordNumber = parRecordnumber;
            this.ReadCurrentRecord();
        }
        public void MovePrevious()
        {
            if (this.CurrentRecordNumber > 1)
            {
                this.CurrentRecordNumber--;
                this.ReadCurrentRecord();
            }
        }
        public void MoveNext()
        {
            if (this.CurrentRecordNumber <= this.RecordCount)
            {
                this.CurrentRecordNumber++;
                this.ReadCurrentRecord();
            }
        }
        public void MoveFirst()
        {
            this.CurrentRecordNumber = 1;
            this.ReadCurrentRecord();
        }
        public void MoveLast()
        {
            this.CurrentRecordNumber = this.RecordCount;
            this.ReadCurrentRecord();
        }
        #endregion

        #region AddNew/Edit/Update/Delete/CancelUpdate
        public void AddNew()
        {
            this.CurrentRecordNumber = 0;
            this.State = enmRecordsetState.Appending;
            this.Fields.RollbackToInitValues();
        }
        public void Edit()
        {
            this.State = enmRecordsetState.Editing;
            this.Fields.SubmitValues();
        }
        public void Update()
        {
            string varSQL = null;
            int varExecuted = -1;
            switch (this.State)
            {
                case enmRecordsetState.Appending:
                    varSQL = this.getInsertString();
                    if (varSQL != "")
                        varExecuted = this.Connection.Execute(varSQL);
                    break;
                case enmRecordsetState.Editing:
                    varSQL = this.getUpdateString();
                    if (varSQL != "")
                        varExecuted = this.Connection.Execute(varSQL);
                    break;
            }

            if (varExecuted > int.MinValue)
            {
                this.State = enmRecordsetState.Viewing;
                this.Fields.SubmitValues();
            }
        }
        public void Delete()
        {
            if (this.Connection.Execute(this.getDeleteString()) > int.MinValue)
            {
                this.State = enmRecordsetState.Viewing;

                if (this.RecordsDeleted == null)
                    this.RecordsDeleted = new ArrayList();

                this.RecordsDeleted.Add(this.CurrentRecordNumber);
                this.MoveFirst();
            }
        }
        public void CancelUpdate()
        {
            this.Fields.RollbackToOldValues();
            this.State = enmRecordsetState.Viewing;
        }
        #endregion

        #region getRows
        public object[,] getRows()
        {
            return this.getRows(this.RecordCount);
        }
        public object[,] getRows(int parNumberOfRecords)
        {
            return this.getRows(parNumberOfRecords, 0);
        }
        public object[,] getRows(int parNumberOfRecords, int parStartFrom)
        {
            if (parStartFrom > 0)
                this.CurrentRecordNumber = parStartFrom;

            if (parNumberOfRecords > this.RecordCount + this.AbsolutePosition)
                parNumberOfRecords = this.RecordCount + this.AbsolutePosition;

            if (parNumberOfRecords <= 0)
                return null;

            ArrayList arrValues = new ArrayList();
            object[] mtxFieldValues = null;
            for (int varRecordcounter = 1; varRecordcounter <= parNumberOfRecords; varRecordcounter++)
            {
                this.CurrentRecordNumber = varRecordcounter + parStartFrom;
                if (this.RecordsDeleted == null || !this.RecordsDeleted.Contains(this.CurrentRecordNumber))
                {
                    this.ReadCurrentRecord();
                    mtxFieldValues = new object[this.Fields.Count];
                    for (int varFieldcounter = 0; varFieldcounter < this.Fields.Count; varFieldcounter++)
                    {
                        mtxFieldValues[varFieldcounter] = this.Fields[varFieldcounter + 1].Value;
                    }
                    arrValues.Add(mtxFieldValues);
                }
            }

            if (arrValues.Count == 0)
                return null;

            object[,] mtxValues = new object[this.Fields.Count, arrValues.Count];
            for (int varRecordcounter = 0; varRecordcounter < arrValues.Count; varRecordcounter++)
            {
                mtxFieldValues = (object[])arrValues[varRecordcounter];
                for (int varFieldcounter = 0; varFieldcounter < this.Fields.Count; varFieldcounter++)
                {
                    mtxValues[varFieldcounter, varRecordcounter] = mtxFieldValues[varFieldcounter];
                }
            }

            arrValues.Clear();

            return mtxValues;
        }
        #endregion

        #region getAddString/getUpdateString/getDeleteString
        internal virtual string getInsertString()
        {
            string varSelectedDatabasePart = this.Connection.SelectedDatabase;
            if (varSelectedDatabasePart != "")
                varSelectedDatabasePart = varSelectedDatabasePart + ".";

            return "INSERT " + " INTO " + varSelectedDatabasePart + this.Tablename + " " + this.Fields.getInsertString();
        }
        internal string getUpdateString()
        {
            string varUpdateString = this.Fields.getUpdateString();
            if (varUpdateString == "")
                return "";
            else
            {
                string varSelectedDatabasePart = this.Connection.SelectedDatabase;
                if (varSelectedDatabasePart != "")
                    varSelectedDatabasePart = varSelectedDatabasePart + ".";

                return "UPDATE " + varSelectedDatabasePart + this.Tablename + varUpdateString;
            }
        }
        internal string getDeleteString()
        {
            if (this.CurrentRecord == null)
                return "";
            else
            {
                string varSelectedDatabasePart = this.Connection.SelectedDatabase;
                if (varSelectedDatabasePart != "")
                    varSelectedDatabasePart = varSelectedDatabasePart + ".";

                return "DELETE FROM " + varSelectedDatabasePart + this.Tablename + " WHERE " + this.Fields.getPrimaryKeyWhere();
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.Connection = null;
            this.SQL = null;
            try
            {
                this.CloseRecordset();
            }
            catch
            { }
            this.State = enmRecordsetState.Viewing;
            GC.SuppressFinalize(this);
        }
        private void DisposeCurrentRecord()
        {
            if (this.CurrentRecord != null)
            {
                this.CurrentRecord.Dispose();
                this.CurrentRecord = null;
            }
        }
        private void DisposeDeletedRecords()
        {
            if (this.RecordsDeleted != null)
            {
                this.RecordsDeleted.Clear();
                this.RecordsDeleted = null;
            }
        }
        #endregion

        internal SQLRecordset(SQLConnection parConnection)
        {
            this.Connection = parConnection;
        }
    }
}
