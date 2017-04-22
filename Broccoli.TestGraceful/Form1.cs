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
            var brad = new Person
            {
                Name = "Brad Jones",
                Age = 27,
                HomeAddress = new Address
                {
                    StreetNo = 123,
                    StreetName = "Fake St",
                    City = "Virtual Land"
                }
            };

            brad.Save();
        }
    }
}
