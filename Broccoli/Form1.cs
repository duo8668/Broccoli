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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Broccoli.Core.Database.Eloquent;
using Broccoli.POC;

namespace Broccoli
{
    // Form tester
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DbFacade.Initialize();
            MessageWorkerFacade.Initialize();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int run = 5;
            var time1 = DateTime.Now;
            label1.Text = time1.ToString("HH:mm:ss.fffff");
            BroccoDbTester bdt = new BroccoDbTester();
            var search = bdt.pureQueryPerformancetest();
          
            Parallel.For(0, run, (ssss) =>
            {
                bdt.explicitlySavePerformanceTest(search);
                // explicitlySavePerformanceTest(search);
                // testRabbitMQPub(rabbitChannel);
            });

            var time2 = DateTime.Now;
            label2.Text = time2.ToString("HH:mm:ss.fffff");

            var time3 = time2 - time1;
            label3.Text = "" + time3.TotalMilliseconds + " ::   Avg: " + (time3.TotalMilliseconds / run);

        }
        
        private string testArgs(string main = "", params object[] args)
        {
            foreach (var arg in args)
            {

            }

            return main;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
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
