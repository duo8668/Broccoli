using Broccoli.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Broccoli
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Invoice inv = Invoice.Find("1");
            long firstLong = DateTime.Now.Ticks;
            label1.Text = "" + firstLong;
            System.Threading.Thread.Sleep(1000);
            long secondLong = DateTime.Now.Ticks;
            label2.Text = "" + secondLong;
            long longDiff = secondLong - firstLong;
            label3.Text = string.Format("{0}   {1}：{2}", longDiff, (new DateTime(longDiff)).Second, (new DateTime(longDiff)).Ticks);
        }
    }
}
