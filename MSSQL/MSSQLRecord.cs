using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
