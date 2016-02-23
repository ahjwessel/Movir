using System;

namespace Common.Templates
{
    public abstract class Table : IDisposable
    {
        public string Tablename { get; private set; }
        protected Fields Fields { get; private set; }
        public virtual bool IsSaved
        {
            get
            {
                return !this.IsNew && !this.IsDirty;
            }
        }
        public bool IsNew { get; private set; }
        public bool IsFound { get; private set; }
        public virtual bool IsDirty
        {
            get
            {
                return this.Fields.IsDirty;
            }
        }
        public bool IsAlreadyInDatabase
        {
            get
            {
                return (!this.IsNew && this.IsFound);
            }
        }

        protected void SubmitValues()
        {
            this.Fields.SubmitValues();
        }
        protected virtual void RollbackToInitValues()
        {
            this.Fields.RollbackToInitValues();
        }
        public virtual void CreateNew()
        {
            this.RollbackToInitValues();
            this.SetFlags(true, true);
        }
        public virtual void CreateCopy()
        {
            this.SetFlags(true, true);
        }
        protected void SetFlags(bool parIsNew, bool parIsFound)
        {
            this.IsNew = parIsNew;
            this.IsFound = parIsFound;
        }

        public virtual void Dispose()
        {
            this.Tablename = null;
            this.Fields.Dispose();
        }

        protected Table(string parTablename, Fields parFields)
        {
            this.Tablename = parTablename;
            this.Fields = parFields;

            this.CreateNew();
        }
    }
}
