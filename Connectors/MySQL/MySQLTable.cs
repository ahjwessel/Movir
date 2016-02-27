using Common.Data;
using System.Text;

namespace MySQL
{
    public class MySQLTable:SQLTable
    {
        protected bool IsTempTable { get; set; }
        protected override void pCreateTable(SQLConnector connector, params SQLIndex[] indexes)
        {
            System.Text.StringBuilder sbFields = new System.Text.StringBuilder();
            foreach (MySQLField field in this.Fields)
            {
                sbFields.Append("," + field.CreateLine);
            }

            StringBuilder sbIndexes = new StringBuilder();
            sbIndexes.Append("," + new MySQLIndex(SQLIndexTypes.PrimaryKey, "", this.Fields.GetPrimaryKeys()).CreateLine);
            if (indexes != null && indexes.Length > 0)
            {
                foreach (MySQLIndex ind in indexes)
                {
                    sbIndexes.Append("," + ind.CreateLine);
                }
            }

            string sqlstr = "CREATE " + (this.IsTempTable ? "TEMPORARY " : "") + "TABLE `" + this.Tablename + "` (" +
                            sbFields.ToString().Substring(1) +
                            "," + sbIndexes.ToString().Substring(1) +
                            ") " +
                            "ENGINE = MyISAM " +
                            "CHARACTER SET utf8 COLLATE utf8_general_ci;";
            connector.Execute(sqlstr);
        }
        public MySQLTable(string tablename, params MySQLField[] parFields)
            : base(tablename,new MySQLFields(parFields))
        { }
    }
}
