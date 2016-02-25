using System;
using Common.Data;
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
                foreach (string name in this.FieldNames)
                {
                    if (Names.Length > 0)
                        Names.Append(",");
                    Names.Append(name);
                }

                return BaseIndex.ToString() + " ON " + this.Tablename + "(" + Names.ToString() + ") ";
            }
        }

        public MSSQLIndex(SQLIndexTypes indexType, string name,
                          params string[] fieldnames)
            :base(indexType,name,fieldnames)
        {}
        public MSSQLIndex(SQLIndexTypes indexType, string fieldname)
		:this(indexType,fieldname,fieldname)
		{ }
        public MSSQLIndex(string fieldName)
		:this(SQLIndexTypes.NormalIndex,fieldName)
		{ }
        public MSSQLIndex(string name, params string[] fieldnames)
		:this(SQLIndexTypes.NormalIndex,name,fieldnames)
		{ }
    }
}
