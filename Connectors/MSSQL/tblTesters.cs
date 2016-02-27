using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MySQL
{
    public class tblTesters : MySQLTable
    {
        public string Naam
        {
            get
            {
                return (string)this.Fields["Naam"].Value;
            }
            set
            {
                this.Fields["Naam"].Value = value;
            }
        }
        new public MySQLFields Fields
        {
            get
            {
                return (MySQLFields)base.Fields;
            }
        }
        public static void CreateTable(MySQLConnector connector)
        {
            MySQLTable.CreateTable(connector, typeof(tblTesters));
        }

        public void TestRead(MySQLConnector connector)
        {
            this.pRead(connector, this.GetSQL());
        }

        public tblTesters() : 
            base("tblTesters", new MySQLAutonumberField("ID"), 
                new MySQLTextField("Naam",255,""),
                new MySQLField("Nummer",MySqlDbType.Int24,0),
                new MySQLField("Datum", MySqlDbType.Date,DateTime.Now.Date),
                new MySQLField("Decimal", MySqlDbType.Float,0),
                new MySQLBinaryField("Binary"),
                new MySQLImageField("Image"))
        {}
    }
}
