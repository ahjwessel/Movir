using Common.Templates;
using System.Data;

namespace MSSQL
{
    public class MSSQLRecord:SQLRecord
    {
        public MSSQLRecord(DataTable parSchemaTable, DataSet parDataSet)
            :base(new MSSQLFields(parSchemaTable, parDataSet))
        {}
    }
}
