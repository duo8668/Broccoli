using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Graceful.Dynamic;
using System.Configuration;

namespace Broccoli.TestGraceful
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var brad = BioBill.Where(iii => iii.BillNumber == "B102010000003", true).First();

            var dtls = brad.BioBillDetails;
            button1.Text = "dddddddddddddddd";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Graceful.Context.Connect(ConfigurationManager.ConnectionStrings["BBranch101"].ConnectionString);
        }
    }
}
