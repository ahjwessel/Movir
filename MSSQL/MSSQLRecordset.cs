using System.Data;
using Common.Data;

namespace MSSQL
{
    public class MSSQLRecordset : SQLRecordset
    {
        protected override SQLRecord CreateRecord(DataTable schemaTable, DataSet dataSet)
        {
            return new MSSQLRecord(schemaTable, dataSet);
        }

        public MSSQLRecordset(MSSQLConnector parConnection)
            :base(parConnection)
        { }
    }
}
