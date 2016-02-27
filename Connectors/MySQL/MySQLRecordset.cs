using System.Data;
using Common.Data;

namespace MySQL
{
    public class MySQLRecordset : SQLRecordset
    {
        protected override SQLRecord CreateRecord(DataTable schemaTable, DataSet dataSet)
        {
            return new MySQLRecord(schemaTable, dataSet);
        }

        public MySQLRecordset(MySQLConnector parConnection)
            :base(parConnection)
        { }
    }
}
