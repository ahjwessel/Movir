using Common.Data;

namespace MSSQL
{
    public class MSSQLTable:SQLTable
    { 
        protected override void pCreateTable(SQLConnector connector, params SQLIndex[] indexes)
        {
            System.Text.StringBuilder fields = new System.Text.StringBuilder();
            foreach (MSSQLField field in this.Fields)
            {
                if (fields.Length > 0)
                    fields.Append(",");
                fields.Append(field.CreateLine);
            }

            string sqlstr = "CREATE TABLE " + this.Tablename + " (" +
                            fields.ToString() +
                            ") ";
            connector.Execute(sqlstr);

            System.Text.StringBuilder sbIndexes = new System.Text.StringBuilder();
            if (indexes != null && indexes.Length > 0)
            {
                foreach (MSSQLIndex ind in indexes)
                {
                    ind.Tablename = this.Tablename;
                    connector.Execute(ind.CreateLine);
                }
            }
        }
        public MSSQLTable(string tablename, params MSSQLField[] parFields)
            : base(tablename,new MSSQLFields(parFields))
        { }
    }
}
