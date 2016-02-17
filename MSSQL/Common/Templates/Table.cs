using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected virtual void RollbackToInitValues()
        {
            this.EmptyValues();
        }
        protected virtual void EmptyValues()
        {
            this.Fields.RollbackToInitValues();
        }
        public virtual void CreateNew()
        {
            this.EmptyValues();
            this.SetFlags(true, true);
        }

        public virtual void CreateCopy()
        {
            this.SetFlags(true, true);
        }

        protected string[] getFieldnames()
        {
            string[] mtx = new string[this.Fields.Count];

            for (int varCounter = 0; varCounter < this.Fields.Count; varCounter++)
            {
                mtx[varCounter] = this.Fields[varCounter].Name;
            }
            return mtx;
        }
        protected void SetFlags(bool parIsNew, bool parIsFound)
        {
            this.IsNew = parIsNew;
            this.IsFound = parIsFound;
        }
        protected void SubmitValues()
        {
            this.Fields.SubmitValues();
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
