using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Facade
{
    public class MessageWorkerFacade
    {
        private static Dictionary<string,ConnectionFactory> _connFactories;
        private static Dictionary<string, IModel> _modelChannels;
        public MessageWorkerFacade()
        {
            var _channel = CreateChannel();
        }

        public IModel CreateChannel(string hostName = "localhost")
        {
            IModel channel;
            var factory = new ConnectionFactory() { HostName = hostName };
            var connection = factory.CreateConnection();

            channel = connection.CreateModel();

            return channel;
        }

        public void CloseChannel()
        {
           // _channel.Close();
           // _channel.Dispose();
        }



        public void CreateExchange()
        {
            /*
            channel.ExchangeDeclare("logs", "fanout");
            */
        }

        public void CreateQueue(IModel channel, string _queue)
        {
            if (channel != null && !string.IsNullOrEmpty(_queue))
            {
                var queueDeclare = channel.QueueDeclare(queue: _queue
                    , durable: true
                    , exclusive: false
                    , autoDelete: false
                    , arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            }
        }

        public static void Initialize()
        {
            _connFactories = new Dictionary<string, ConnectionFactory>();
            _modelChannels = new Dictionary<string, IModel>();

        }
    }
}
