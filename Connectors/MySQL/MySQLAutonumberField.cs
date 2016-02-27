using MySql.Data.MySqlClient;
using System.Data;

namespace MySQL
{
    public class MySQLAutonumberField:MySQLField
    {
        public override string CreateLine
        {
            get
            {
                return this.Name + " " + this.Type.ToString()+ " IDENTITY(1,1) " + (this.IsPrimaryKey ? "PRIMARY KEY " : "");
            }
        }

        public MySQLAutonumberField(string name)
            :base(name, MySqlDbType.Int32,0,true,true,false)
        { }
    }
}
