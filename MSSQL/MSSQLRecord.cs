using Common.Data;
using System.Data;

namespace MSSQL
{
    public class MSSQLRecord:SQLRecord
    {
        public MSSQLRecord(DataTable schemaTable, DataSet dataSet)
            :base(new MSSQLFields(schemaTable, dataSet))
        {}
    }
}
