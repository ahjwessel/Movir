using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Divers_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var d = new Dictionary<int, Control>();
            var rnd = new Random();
            for (int varCounter=0;varCounter<1000;varCounter++)
            {
                try
                {
                    d.Add(rnd.Next(1, int.MaxValue), button1);
                }
                catch{ }
            }

            rnd.ToString();
        }
    }
}
