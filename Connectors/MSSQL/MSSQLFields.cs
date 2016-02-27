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
            for (int counter = 0; counter < schemaTable.Rows.Count; counter++)
            {
                _fields[counter] = new MSSQLField(counter, schemaTable);
            }
        }
    }
}
