using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Common.Data
{
    public enum RecordsetStates : byte
    {
        Viewing = 0,
        Appending = 1,
        Editing = 2,
        Deleted = 3
    }

    public abstract class SQLRecordset:IDisposable
    {
        public SQLConnector Connection { get; protected set; }
        public DataTable SchemaTable{get; protected set;}
        public DataSet DataSet { get; protected set; }
        public string Tablename { get; protected set; }
        public string SQL { get; protected set; }
        public int RecordCount { get; protected set; }
        public int CurrentRecordNumber{get; protected set;}
        protected SQLRecord CurrentRecord { get; set; }
        protected List<int> RecordsDeleted { get; private set; }
        public RecordsetStates State { get; protected set; }

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
        protected internal bool OpenRecordset(string SQL)
        {
            this.SQL = SQL;
            return this.Requery();
        }
        public bool Requery()
        {
            this.CloseRecordset(false);

            Exception exception = null;
            var command = this.Connection.CreateCommand(this.SQL);
            command.CommandTimeout = this.Connection.TimeoutOpenRecordsetInSeconden;
            DbDataReader reader = command.ExecuteReader(CommandBehavior.KeyInfo);
            try
            {
                this.SchemaTable = reader.GetSchemaTable();
                this.DataSet = new DataSet();
                this.DataSet.EnforceConstraints = false;
                this.DataSet.Load(reader, LoadOption.OverwriteChanges, "tbl");
                this.RecordCount = this.DataSet.Tables[0].Rows.Count;
                this.Tablename = this.SchemaTable.Rows[0]["BaseTableName"].ToString();
                this.CurrentRecord = this.CreateRecord(this.SchemaTable, this.DataSet);
                if (this.RecordCount > 0)
                    this.MoveFirst();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            reader.Close();
            reader.Dispose();
            command.Dispose();

            if (exception != null)
                throw exception;

            return true;
        }
        public void CloseRecordset()
        {
            this.CloseRecordset(true);
        }
        private void CloseRecordset(bool resetSQL)
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
            this.RecordsDeleted.Clear();

            if (resetSQL)
                this.SQL = "";
        }
        protected abstract SQLRecord CreateRecord(DataTable schemaTable, DataSet dataSet);
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
                this.State = RecordsetStates.Deleted;
            }
        }

        #region Move... voids
        public void MoveToRecord(int recordnumber)
        {
            this.CurrentRecordNumber = recordnumber;
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
            this.State = RecordsetStates.Appending;
            this.Fields.RollbackToInitValues();
        }
        public void Edit()
        {
            this.State = RecordsetStates.Editing;
            this.Fields.SubmitValues();
        }
        public void Update()
        {
            int varExecuted = -1;
            switch (this.State)
            {
                case RecordsetStates.Appending:
                    varExecuted = this.Connection.Execute(this.Fields.GetInsertString(this.Tablename));
                    break;
                case RecordsetStates.Editing:
                    varExecuted = this.Connection.Execute(this.Fields.GetUpdateString(this.Tablename));
                    break;
            }

            if (varExecuted > int.MinValue)
            {
                this.State = RecordsetStates.Viewing;
                this.Fields.SubmitValues();
            }
        }
        public void Delete()
        {
            if (this.Connection.Execute(this.Fields.GetDeleteString(this.Tablename)) > int.MinValue)
            {
                this.State = RecordsetStates.Viewing;

                this.RecordsDeleted.Add(this.CurrentRecordNumber);
                this.MoveFirst();
            }
        }
        public void CancelUpdate()
        {
            this.Fields.RollbackToOldValues();
            this.State = RecordsetStates.Viewing;
        }
        #endregion

        #region GetRows
        public object[,] GetRows()
        {
            return this.GetRows(this.RecordCount);
        }
        public object[,] GetRows(int numberOfRecords)
        {
            return this.GetRows(numberOfRecords, 0);
        }
        public object[,] GetRows(int numberOfRecords, int startFrom)
        {
            if (startFrom > 0)
                this.CurrentRecordNumber = startFrom;

            if (numberOfRecords > this.RecordCount + this.AbsolutePosition)
                numberOfRecords = this.RecordCount + this.AbsolutePosition;

            if (numberOfRecords <= 0)
                return null;

            var AllValues = new List<object[]>();
            object[] FieldValues = null;
            for (int varRecordcounter = 1; varRecordcounter <= numberOfRecords; varRecordcounter++)
            {
                this.CurrentRecordNumber = varRecordcounter + startFrom;
                if (!this.RecordsDeleted.Contains(this.CurrentRecordNumber))
                {
                    this.ReadCurrentRecord();
                    FieldValues = this.Fields.GetValues();
                    AllValues.Add(FieldValues);
                }
            }

            if (AllValues.Count == 0)
                return null;

            object[,] mtxValues = new object[this.Fields.Count, AllValues.Count];
            for (int varRecordcounter = 0; varRecordcounter < AllValues.Count; varRecordcounter++)
            {
                FieldValues = (object[])AllValues[varRecordcounter];
                for (int varFieldcounter = 0; varFieldcounter < this.Fields.Count; varFieldcounter++)
                {
                    mtxValues[varFieldcounter, varRecordcounter] = FieldValues[varFieldcounter];
                }
            }

            AllValues.Clear();

            return mtxValues;
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.Connection = null;
            this.SQL = null;
            this.RecordsDeleted.Clear();
            try
            {
                this.CloseRecordset();
            }
            catch
            { }
            this.State = RecordsetStates.Viewing;
        }
        private void DisposeCurrentRecord()
        {
            if (this.CurrentRecord != null)
            {
                this.CurrentRecord.Dispose();
                this.CurrentRecord = null;
            }
        }
        #endregion

        internal SQLRecordset(SQLConnector parConnection)
        {
            this.Connection = parConnection;
            this.RecordsDeleted = new List<int>();
        }
    }
}
