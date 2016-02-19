using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Templates;

namespace MSSQL
{
    public class MSSQLIndex : SQLIndex
    {
        public override string CreateLine
        {
            get
            {
                string varIndexTypeStart = "";
                switch (this.IndexType)
                {
                    case SQLIndexTypes.PrimaryKey:
                        varIndexTypeStart = "PRIMARY KEY ";
                        break;
                    case SQLIndexTypes.NormalIndex:
                        varIndexTypeStart = "INDEX " + this.Name + " ";
                        break;
                    case SQLIndexTypes.UniqueIndex:
                        varIndexTypeStart = "UNIQUE INDEX " + this.Name + " ";
                        break;
                    case SQLIndexTypes.FullText:
                        varIndexTypeStart = "FULLTEXT INDEX " + this.Name + " ";
                        break;
                }
                System.Text.StringBuilder sbNames = new System.Text.StringBuilder();
                string varToAdd = "";
                foreach (string varName in this.FieldNames)
                {
                    if (varName.IndexOf("(") >= 0)
                    {
                        varToAdd = varName.Substring(0, varName.IndexOf("("));
                        varToAdd = "" + varToAdd + "" + varName.Substring(varName.IndexOf("("));
                    }
                    else
                        varToAdd = "" + varName + "";

                    sbNames.Append("," + varToAdd);
                }
                string varTotal = varIndexTypeStart;
                if (sbNames.Length > 0)
                    varTotal += "(" + sbNames.ToString().Substring(1) + ")";

                return varTotal;
            }
        }

        public MSSQLIndex(SQLIndexTypes parIndexType, string parName,
                          params string[] parFieldNames)
            :base(parIndexType,parName,parFieldNames)
        {}
        public MSSQLIndex(SQLIndexTypes parIndexType, string parFieldName)
		:this(parIndexType,parFieldName,parFieldName)
		{ }
        public MSSQLIndex(string parFieldName)
		:this(SQLIndexTypes.NormalIndex,parFieldName)
		{ }
        public MSSQLIndex(string parName, params string[] parFieldNames)
		:this(SQLIndexTypes.NormalIndex,parName,parFieldNames)
		{ }
    }
}
