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
            set
            {
                this.Value = ConvertSQLToValue(value);
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
        protected abstract string ConvertValueToSQL(object parValue);
        protected abstract object ConvertSQLToValue(string parSQLValue);

        public abstract void Refresh(DataRow parRow);

        public SQLField(string parName, bool parAllowDBNull, object parInitValue)
            :base(parName,parInitValue)
        {
            this.AllowDBNull = parAllowDBNull;
        }

        public SQLField(int parFieldnumber, DataTable parSchemaTable)
		:base(parSchemaTable.Rows[parFieldnumber-1]["ColumnName"].ToString(),null)
		{
            this.Fieldnumber = parFieldnumber;
            this.Value = null;
            this.IsPrimaryKey = (bool)parSchemaTable.Rows[parFieldnumber - 1]["IsKey"];
            this.IsAutonumber = (bool)parSchemaTable.Rows[parFieldnumber - 1]["IsAutoIncrement"];
            this.AllowDBNull = (bool)parSchemaTable.Rows[parFieldnumber - 1]["AllowDBNull"];
        }
    }
}
