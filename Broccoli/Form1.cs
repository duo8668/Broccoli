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
using StackExchange.Redis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
            DbFacade.Initialize();
            MessageWorkerFacade.Initialize();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int run = 1;
            var time1 = DateTime.Now;
            label1.Text = time1.ToString("HH:mm:ss.fffff");

            var rabbitChannel = testInitRabbitMQChannel();
            //testRabbitMQConsume(rabbitChannel);
            var search = pureQueryPerformancetest();
            explicitlySavePerformanceTest(search);
            search.Dispose();
            Parallel.For(0, run, (ssss) =>
            {
                //   explicitlySavePerformanceTest(search);
                //testRabbitMQPub(rabbitChannel);
            });

            var time2 = DateTime.Now;
            label2.Text = time2.ToString("HH:mm:ss.fffff");

            var time3 = time2 - time1;
            label3.Text = "" + time3.TotalMilliseconds + " ::   Avg: " + (time3.TotalMilliseconds / run);

        }

        #region RabbitMQ TEST
        private IModel testInitRabbitMQChannel()
        {
            IModel channel;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();

            channel = connection.CreateModel();
            /*
            channel.ExchangeDeclare("logs", "fanout");
            */
            channel.QueueDeclare(queue: "task_queue",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            return channel;

        }
        public void testRabbitMQPub(IModel channel)
        {
            // var message = GetMessage(args);
            /* PUBLISH SUBSCBRIBE
            var body = Encoding.UTF8.GetBytes("Hello World!");
            channel.BasicPublish(exchange: "logs",
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);
              */
            var message = "Hello world!";
            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: "task_queue",
                                 basicProperties: properties,
                                 body: body);
            Console.WriteLine(" [x] Sent {0}", message);
        }

        public void testRabbitMQConsume(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                textBox1.Text += string.Format(" [x] Received {0}" + System.Environment.NewLine, message);

            };
            channel.BasicConsume(queue: "task_queue", noAck: true, consumer: consumer);
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
        #endregion

        #region REDIS FAILED PERFORMANCE TEST
        //* REDIS TEST FAILED, memory doesnt pass the test
        private void ConnectRedis()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6650");
        }

        private void testRedis(ConnectionMultiplexer redis)
        {
            redis = ConnectionMultiplexer.Connect("localhost:6650");

            ISubscriber sub = redis.GetSubscriber();
        }
        #endregion

        private void hasManyPerformanceTest(Invoice search)
        {
            var custs = search.hasMany<Customer>((cccc) => cccc.FirstName == "%a%", true).ToList();

        }

        private Invoice pureQueryPerformancetest()
        {
            return Invoice.Find((myInv) => myInv.InvoiceNum == "INV-222222");
            // return DbFacade.GetDatabaseConnection(Invoice.ConnectionName).Query<Invoice>(@"select * from sales__invoice WHERE invoice_num='INV-222222'").SingleOrDefault();
            // return Invoice.Find((lin) => lin.Where((myInv) => myInv.InvoiceNum == "INV-33333"));
        }
        private void pureInsertPerformanceTest()
        {
            var invToAdd = new Invoice();
            invToAdd.InvoiceNum = "INV-555555";
            invToAdd.InvoiceDateTime = DateTime.Parse("2017-06-01 15:22");
            invToAdd.Save();
        }

        private void pureUpdatePerformanceTest(Invoice search)
        {
            search.InvoiceNum = "INV-333333";
            search.Save();
            var stringTest = search.InvoiceNum;
            search.InvoiceNum = "INV-222222";
            search.Save();
        }

        private void explicitlySavePerformanceTest(Invoice search)
        {
            // var custs = search.hasMany<Customer>((cccc) => cccc.FirstName == "%a%", true);

            foreach (var cust in search.Customers)
            {
                cust.LastName += "_";
            }
            search.Save();
            foreach (var cust in search.Customers)
            {
                cust.LastName = cust.LastName.Replace("_", "");
            }
        //    search.Save();
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
