using System.Data;
using Common.Data;

namespace MySQL
{
    public class MySQLFields:SQLFields
    {
        new public MySQLField this[string name]
        {
            get
            {
                return (MySQLField)base[name];
            }
        }
        new public MySQLField this[int index]
        {
            get
            {
                return (MySQLField)base[index];
            }
        }
        public MySQLFields(params MySQLField[] parFields)
            :base(parFields)
        { }

        public MySQLFields(DataTable schemaTable, DataSet dataSet)
        {
            _fields = new MySQLField[schemaTable.Rows.Count];
            for (int counter = 0; counter < schemaTable.Rows.Count; counter++)
            {
                _fields[counter] = new MySQLField(counter, schemaTable);
            }
        }
    }
}
