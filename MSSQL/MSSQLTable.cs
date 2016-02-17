using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Templates;

namespace MSSQL
{
    public class MSSQLTable:Table
    {
        protected string[] PrimaryKeys { get; private set; }

        public MSSQLTable(string parTablename, string parPrimaryKey, params MSSQLField[] parFields)
            :this(parTablename,new string[] { parPrimaryKey },parFields)
        { }
        public MSSQLTable(string parTablename, string[] parPrimaryKeys, params MSSQLField[] parFields)
            : base(parTablename, new MSSQLFields(parFields))
        {
            this.PrimaryKeys = parPrimaryKeys;
        }
    }
}
