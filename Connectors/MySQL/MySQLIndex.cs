using System.Text;
using Common.Data;

namespace MySQL
{
    public class MySQLIndex : SQLIndex
    {
        public override string CreateLine
        {
            get
            {
                var BaseIndex = new StringBuilder();

                switch (this.IndexType)
                {
                    case SQLIndexTypes.PrimaryKey:
                        BaseIndex.Append("PRIMARY KEY ");
                        break;
                    case SQLIndexTypes.UniqueIndex:
                        BaseIndex.Append("UNIQUE INDEX ");
                        break;
                    case SQLIndexTypes.FullText:
                        BaseIndex.Append("FULLTEXT INDEX ");
                        break;
                    default:
                        BaseIndex.Append("INDEX ");
                        break;
                }
                if (this.IndexType != SQLIndexTypes.PrimaryKey)
                {
                    BaseIndex.Append("`");
                    BaseIndex.Append(this.Name);
                    BaseIndex.Append("` ");
                }

                StringBuilder Names = new StringBuilder();
                foreach (string name in this.FieldNames)
                {
                    if (Names.Length > 0)
                        Names.Append(",");
                    Names.Append(name);
                }
                return BaseIndex.ToString() + "(" + Names.ToString() + ") ";
            }
        }
        public MySQLIndex(SQLIndexTypes indexType, string name,
                          params string[] fieldnames)
            :base(indexType,name,fieldnames)
        { }
        public MySQLIndex(SQLIndexTypes indexType, string fieldname)
		:this(indexType,fieldname,fieldname)
		{ }
        public MySQLIndex(string fieldName)
		:this(SQLIndexTypes.NormalIndex,fieldName)
		{ }
        public MySQLIndex(string name, params string[] fieldnames)
		:this(SQLIndexTypes.NormalIndex,name,fieldnames)
		{ }
    }
}
