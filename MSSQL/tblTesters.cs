using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL
{
    public class tblTesters : MSSQLTable
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
        new public MSSQLFields Fields
        {
            get
            {
                return (MSSQLFields)base.Fields;
            }
        }
        public static void CreateTable(MSSQLConnector parConnector)
        {
            MSSQLTable.CreateTable(parConnector, typeof(tblTesters));
        }

        public void TestRead(MSSQLConnector parConnector)
        {
            this.pRead(parConnector, this.getSQL());
        }

        public tblTesters() : 
            base("tblTesters", new MSSQLAutonumberField("ID"), 
                new MSSQLTextField("Naam",255,true,""),
                new MSSQLField("Nummer",SqlDbType.Int,0),
                new MSSQLField("Datum",SqlDbType.Date,DateTime.Now.Date),
                new MSSQLField("Decimal",SqlDbType.Float,0),
                new MSSQLBinaryField("Binary"),
                new MSSQLImageField("Image"))
        {}
    }
}
