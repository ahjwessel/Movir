using System.Data;
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
