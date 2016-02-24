using System.Data;

namespace MSSQL
{
    public class MSSQLAutonumberField:MSSQLField
    {
        public override string CreateLine
        {
            get
            {
                return this.Name + " " + this.Type.ToString()+ " IDENTITY(1,1) " + (this.IsPrimaryKey ? "PRIMARY KEY " : "");
            }
        }

        public MSSQLAutonumberField(string name)
            :base(name,SqlDbType.Int,0,true,true,false)
        { }
    }
}
