using Common.Data;
using System.Data;

namespace MSSQL
{
    public class MSSQLFields:SQLFields
    {
        new public MSSQLField this[string name]
        {
            get
            {
                return (MSSQLField)base[name];
            }
        }
        new public MSSQLField this[int index]
        {
            get
            {
                return (MSSQLField)base[index];
            }
        }
        public MSSQLFields(params MSSQLField[] parFields)
            :base(parFields)
        { }

        public MSSQLFields(DataTable schemaTable, DataSet dataSet)
        {
            _fields = new MSSQLField[schemaTable.Rows.Count];
            for (int varCounter = 0; varCounter < schemaTable.Rows.Count; varCounter++)
            {
                _fields[varCounter] = new MSSQLField(varCounter, schemaTable);
            }
        }
    }
}
