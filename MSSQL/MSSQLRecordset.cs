using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Templates;

namespace MSSQL
{
    public class MSSQLRecordset : SQLRecordset
    {
        protected override SQLRecord CreateRecord(DataTable parSchemaTable, DataSet parDataSet)
        {
            return new MSSQLRecord(parSchemaTable, parDataSet);
        }

        public MSSQLRecordset(MSSQLConnector parConnection)
            :base(parConnection)
        { }
    }
}
