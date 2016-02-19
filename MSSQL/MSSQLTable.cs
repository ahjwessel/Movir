using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Templates;

namespace MSSQL
{
    public class MSSQLTable:SQLTable
    { 
        protected override void pCreateTable(SQLConnector parConnector, params SQLIndex[] parIndexes)
        {
            if (parConnector.HasTable(this.Tablename))
                parConnector.DeleteTable(this.Tablename);

            System.Text.StringBuilder sbFields = new System.Text.StringBuilder();
            foreach (MSSQLField fld in this.Fields)
            {
                sbFields.Append("," + fld.CreateLine);
            }

            string sqlstr = "CREATE TABLE " + this.Tablename + " (" +
                            sbFields.ToString().Substring(1) +
                            ") ";
            parConnector.Execute(sqlstr);

            System.Text.StringBuilder sbIndexes = new System.Text.StringBuilder();
            if (parIndexes != null && parIndexes.Length > 0)
            {
                foreach (MSSQLIndex ind in parIndexes)
                {
                    ind.Tablename = this.Tablename;
                    parConnector.Execute(ind.CreateLine);
                }
            }
        }
        public MSSQLTable(string parTablename, string parPrimaryKey, params MSSQLField[] parFields)
            :this(parTablename,new string[] { parPrimaryKey },parFields)
        { }
        public MSSQLTable(string parTablename, string[] parPrimaryKeys, params MSSQLField[] parFields)
            : base(parTablename,parPrimaryKeys, new MSSQLFields(parFields))
        {
        }
    }
}
