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
            this.Value = this.ConvertSQLToValue(parRow[this.FieldIndex]);
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

        public SQLField(string parName, bool parAllowDBNull, object parInitValue)
            :base(parName,parInitValue)
        {
            this.AllowDBNull = parAllowDBNull;
        }
        public SQLField(int parFieldnumber, DataTable parSchemaTable)
        : base(parSchemaTable.Rows[parFieldnumber - 1]["ColumnName"].ToString(), null)
        {
            this.Fieldnumber = parFieldnumber;
            this.Value = null;

            this.IsPrimaryKey = getBool(parSchemaTable.Rows[parFieldnumber - 1]["IsKey"]);
            this.IsAutonumber = getBool(parSchemaTable.Rows[parFieldnumber - 1]["IsAutoIncrement"]);
            this.AllowDBNull = getBool(parSchemaTable.Rows[parFieldnumber - 1]["AllowDBNull"]);
        }
    }
}
