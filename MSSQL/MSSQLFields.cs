using Common.Templates;
using System.Data;

namespace MSSQL
{
    public class MSSQLFields:SQLFields
    {
        new public MSSQLField this[string parName]
        {
            get
            {
                return (MSSQLField)base[parName];
            }
        }
        new public MSSQLField this[int parIndex]
        {
            get
            {
                return (MSSQLField)base[parIndex];
            }
        }
        public MSSQLFields(params MSSQLField[] parFields)
            :base(parFields)
        { }

        public MSSQLFields(DataTable parSchemaTable, DataSet parDataSet)
        {
            _fields = new MSSQLField[parSchemaTable.Rows.Count];
            for (int varCounter = 0; varCounter < parSchemaTable.Rows.Count; varCounter++)
            {
                _fields[varCounter] = new MSSQLField(varCounter, parSchemaTable);
            }
        }
    }
}
