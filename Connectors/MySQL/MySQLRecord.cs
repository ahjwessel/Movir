using Common.Data;
using System.Data;

namespace MySQL
{
    public class MySQLRecord:SQLRecord
    {
        public MySQLRecord(DataTable schemaTable, DataSet dataSet)
            :base(new MySQLFields(schemaTable, dataSet))
        { }
    }
}
