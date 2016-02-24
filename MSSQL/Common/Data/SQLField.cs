using System.Data;

namespace Common.Data
{
    public abstract class SQLField:Field
    {
        public int FieldIndex { get; private set; }

        public string SQLValue
        {
            get
            {
                return ConvertValueToSQL(base.Value);
            }
        }
        public string SQLOldValue
        {
            get
            {
                return ConvertValueToSQL(base.OldValue);
            }
        }
        public string SQLInitValue
        {
            get
            {
                return ConvertValueToSQL(base.InitValue);
            }
        }
        public bool IsPrimaryKey { get; private set; }
        public bool IsAutonumber { get; private set; }
        public bool AllowDBNull { get; private set; }
        public abstract string CreateLine { get; }
        protected abstract string ConvertValueToSQL(object value);
        protected abstract object ConvertSQLToValue(object SQLValue);

        public void Refresh(DataRow row)
        {
            try
            {
                base.Value = this.ConvertSQLToValue(row[this.FieldIndex]);
            }
            catch
            { }
            
            this.SubmitValue();
        }
        private static bool GetBool(object value)
        {
            try
            {
                return (bool)value;
            }
            catch
            {
                return false;
            }
        }

        public SQLField(int fieldIndex, DataTable schemaTable)
        : this(schemaTable.Rows[fieldIndex]["ColumnName"].ToString(), null,
               GetBool(schemaTable.Rows[fieldIndex]["IsKey"]),
               GetBool(schemaTable.Rows[fieldIndex]["IsAutoIncrement"]),
               GetBool(schemaTable.Rows[fieldIndex]["AllowDBNull"]))
        {
            this.FieldIndex = fieldIndex;
            this.Value = null;
            this.SubmitValue();
        }
        public SQLField(string name, object InitValue, bool isPrimaryKey, bool isAutonumber, bool allowDBNull)
            : base(name, InitValue)
        {
            this.IsPrimaryKey = isPrimaryKey;
            this.IsAutonumber = isAutonumber;
            this.AllowDBNull = allowDBNull;
        }
    }
}
