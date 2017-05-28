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
            Invoice inv = Invoice.Find((myInv) => myInv.InvoiceNum == "INV-222222");
            Customer cust = Customer.Find((cccc) => cccc.id == 1);
            label1.Text = inv.InvoiceNum;
            label2.Text = inv.ModifiedAt.ToShortDateString();
            //var invs = Invoice.QueryAll();
            //inv = Invoice.Find((lin) => lin.Where((myInv) => myInv.InvoiceNum == "INV-222222"));

            var cc = inv.hasMany<Customer>((cccc) => cccc.FirstName == "God", true);
            /*
            Parallel.ForEach(invs, (iiiiii) =>
            {
                var cc = iiiiii.hasMany<Customer>((cust) => cust.FirstName == "God", true);

                foreach (var cust in cc)
                {

                }
            });
          */
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

        private string testArgs(string main = "", params object[] args)
        {
            foreach (var arg in args)
            {

            }

            return main;
        }
    }

    // Define some classes
    public class Student
    {
        public string First { get; set; }
        public string Last { get; set; }
        public int ID { get; set; }
        public List<int> Scores;
        public ContactInfo GetContactInfo(List<ContactInfo> contactList, int id)
        {
            ContactInfo cInfo =
                (from ci in contactList
                 where ci.ID == id
                 select ci)
                .FirstOrDefault();

            return cInfo;
        }

        public override string ToString()
        {
            return First + " " + Last + ":" + ID;
        }
    }
    public class ContactInfo
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public override string ToString() { return Email + "," + Phone; }
    }

    public class ScoreInfo
    {
        public double Average { get; set; }
        public int ID { get; set; }
    }
}
