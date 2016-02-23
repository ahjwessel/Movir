using System;
using Common.Templates;
using System.Text;

namespace MSSQL
{
    public class MSSQLIndex : SQLIndex
    {
        public string Tablename { get; internal set; }
        public override string CreateLine
        {
            get
            {
                var BaseIndex = new StringBuilder();
                BaseIndex.Append("CREATE ");

                switch (this.IndexType)
                {
                    case SQLIndexTypes.PrimaryKey:
                        throw new NotImplementedException();//Wordt gedaan dmv create field
                    case SQLIndexTypes.UniqueIndex:
                        BaseIndex.Append("UNIQUE INDEX ");
                        break;
                    default:
                        BaseIndex.Append("INDEX ");
                        break;
                }

                BaseIndex.Append(this.Name);
                BaseIndex.Append(" ");

                StringBuilder Names = new StringBuilder();
                foreach (string varName in this.FieldNames)
                {
                    if (Names.Length > 0)
                        Names.Append(",");
                    Names.Append(varName);
                }

                return BaseIndex.ToString() + " ON " + this.Tablename + "(" + Names.ToString() + ") ";
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
