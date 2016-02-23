using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSSQL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var sql = new MSSQLConnector();
            sql.OpenConnection("LOCALHOST", 0, "", "");
            if (sql.HasDatabase("test"))
                sql.SelectDatabase("test");
            else
                sql.CreateDatabase("test");
            
            using (var tbl = new tblTesters())
            {
                var mem = new System.IO.MemoryStream();
                pic.Image.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);

                tblTesters.CreateTable(sql);
                tbl.Naam = "blablablabla";
                tbl.Fields["nummer"].Value = DateTime.Now.Minute;
                tbl.Fields["Decimal"].Value = 4.5;
                tbl.Fields["Binary"].Value = new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes("test"));
                tbl.Fields["Image"].Value = mem;
                tbl.Save(sql);

                tbl.TestRead(sql);
                pic.Image = new Bitmap((System.IO.MemoryStream)tbl.Fields["Image"].Value);
            }

            return;
        }
    }
}
