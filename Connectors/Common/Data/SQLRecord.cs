using System;
using System.Data;

namespace Common.Data
{
    public abstract class SQLRecord:IDisposable
    {
        public SQLFields Fields { get; protected set; }

        internal void Refresh(DataRow row)
        {
            if (this.Fields != null)
                this.Fields.Refresh(row);
        }
        internal void ClearValues()
        {
            if (this.Fields != null)
                this.Fields.RollbackToInitValues();
        }

        public void Dispose()
        {
            this.Fields.Dispose();
        }

        public SQLRecord(SQLFields parFields)
        {
            this.Fields = parFields;
        }
    }
}
