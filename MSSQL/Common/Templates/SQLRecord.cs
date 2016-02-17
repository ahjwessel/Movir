﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Templates
{
    public abstract class SQLRecord:IDisposable
    {
        public SQLFields Fields { get; protected set; }

        internal void Refresh(DataRow parRow)
        {
            if (this.Fields != null)
                this.Fields.Refresh(parRow);
        }
        internal void ClearValues()
        {
            if (this.Fields != null)
                this.Fields.RollbackToInitValues();
        }

        public void Dispose()
        {
            if (this.Fields != null)
            {
                this.Fields.Dispose();
                this.Fields = null;
            }
        }

        public SQLRecord(SQLFields parFields)
        {
            this.Fields = parFields;
        }
    }
}