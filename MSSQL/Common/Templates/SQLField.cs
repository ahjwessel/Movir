using System.Data;

namespace Common.Templates
{
    public abstract class SQLField:Field
    {
        #region Properties
        public int FieldIndex { get; protected set; }

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
                base.Value = this.ConvertSQLToValue(parRow[this.FieldIndex]);
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

        public SQLField(int parFieldIndex, DataTable parSchemaTable)
        : this(parSchemaTable.Rows[parFieldIndex]["ColumnName"].ToString(), null,
               getBool(parSchemaTable.Rows[parFieldIndex]["IsKey"]),
               getBool(parSchemaTable.Rows[parFieldIndex]["IsAutoIncrement"]),
               getBool(parSchemaTable.Rows[parFieldIndex]["AllowDBNull"]))
        {
            this.FieldIndex = parFieldIndex;
            this.Value = null;
            this.SubmitValue();
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
