using Broccoli.Core.Entities;
using Broccoli.Core.Facade;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
            /*
            //Invoice inv = Invoice.Find("1");
            long firstLong = DateTime.Now.Ticks;
            label1.Text = "" + firstLong;
            System.Threading.Thread.Sleep(1000);
            long secondLong = DateTime.Now.Ticks;
            label2.Text = "" + secondLong;
            long longDiff = secondLong - firstLong;
            label3.Text = string.Format("{0}   {1}：{2}", longDiff, (new DateTime(longDiff)).Second, (new DateTime(longDiff)).Ticks);

            var connStrings = System.Configuration.ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connString in connStrings)
            {

            }
            */
            DbFacade.Initialize();
            ModelFacade.LoadClassConfig();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var from = DateTime.Parse("2017-05-06");
            Invoice search = new Invoice();
            Invoice inv = Invoice.Find("invoice_num=@0", false, "INV-222222");
            var invs = Invoice.QueryAll();

            Parallel.ForEach(invs, (iiiiii) =>
            {
                //var custs = iiiiii.Customers;
                var cc = iiiiii.hasMany<Customer>();

                foreach(var cust in cc)
                {

                }
            });

            label1.Text = inv.InvoiceNum;
            label2.Text = inv.ModifiedAt.ToShortDateString();
            if (inv.InvoiceDateTime.HasValue)
            {
                label3.Text = inv.InvoiceDateTime.Value.ToShortDateString();
            }
            else
            {
                label3.Text = "--------------------------";
            }

        }
    }
}
