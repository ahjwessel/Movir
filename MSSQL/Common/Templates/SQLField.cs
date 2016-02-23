using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Templates
{
    public abstract class SQLField:Field
    {
        #region Properties
        public int FieldIndex
        {
            get
            {
                return this.Fieldnumber - 1;
            }
        }
        public int Fieldnumber { get; protected set; }

        public string SQLValue
        {
            get
            {
                return ConvertValueToSQL(this.Value);
            }
        }
        public string SQLOldValue
        {
            get
            {
                return ConvertValueToSQL(this.OldValue);
            }
        }
        public string SQLInitValue
        {
            get
            {
                return ConvertValueToSQL(this.InitValue);
            }
        }
        public bool IsPrimaryKey { get; protected set; }
        public bool IsAutonumber { get; protected set; }
        public bool AllowDBNull { get; protected set; }
        public abstract string CreateLine { get; }
        #endregion
        protected abstract string ConvertValueToSQL(object parValue);
        protected abstract object ConvertSQLToValue(object parSQLValue);

        public void Refresh(DataRow parRow)
        {
            try
            {
                this.Value = this.ConvertSQLToValue(parRow[this.FieldIndex]);
            }
            catch
            { }
            
            this.SubmitValue();
        }
        private static bool getBool(object parValue)
        {
            try
            {
                return (bool)parValue;
            }
            catch
            {
                return false;
            }
        }

        public SQLField(int parFieldnumber, DataTable parSchemaTable)
        : this(parSchemaTable.Rows[parFieldnumber - 1]["ColumnName"].ToString(), null,
               getBool(parSchemaTable.Rows[parFieldnumber - 1]["IsKey"]),
               getBool(parSchemaTable.Rows[parFieldnumber - 1]["IsAutoIncrement"]),
               getBool(parSchemaTable.Rows[parFieldnumber - 1]["AllowDBNull"]))
        {
            this.Fieldnumber = parFieldnumber;
            this.Value = null;
        }
        public SQLField(string parName, object parInitValue, bool parIsPrimaryKey, bool parIsAutonumber, bool parAllowDBNull)
            : base(parName, parInitValue)
        {
            this.IsPrimaryKey = parIsPrimaryKey;
            this.IsAutonumber = parIsAutonumber;
            this.AllowDBNull = parAllowDBNull;
        }
    }
}
