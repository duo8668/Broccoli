using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.POC
{
    public class RabbitMQChannel
    {

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
               // textBox1.Text += string.Format(" [x] Received {0}" + System.Environment.NewLine, message);
            };

            channel.BasicConsume(queue: "task_queue", noAck: true, consumer: consumer);
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
        #endregion
    }
}
