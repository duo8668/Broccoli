using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Broccoli.Core.Configuration
{
    public class TaskQueueConfiguration
    {
        public static Dictionary<string, TaskQueueConfig> _configs;

        public static Dictionary<string, TaskQueueConfig> Configs
        {
            get
            {
                if (_configs == null)
                {
                    _configs = Deserialize("TaskQueue.config");
                }
                return _configs;
            }
        }
        public static void Serialize(string file, DbSchemaConfiguration c)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(c.GetType());
            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, c);
            writer.Flush();
            writer.Close();
        }
        public static Dictionary<string, TaskQueueConfig> Deserialize(string file)
        {
            Dictionary<string, TaskQueueConfig> _dict = new Dictionary<string, TaskQueueConfig>();
            XmlReader reader = XmlReader.Create(file);
            XPathDocument document = new XPathDocument(reader);
            XPathNavigator navigator = document.CreateNavigator();

            foreach (XPathNavigator nav in navigator.Select("/*/TaskQueues/TaskQueue"))
            {
                var config = new TaskQueueConfig();

                string keyValue = nav.GetAttribute("QueueName", navigator.NamespaceURI);
                if (!string.IsNullOrEmpty(keyValue))
                {
                    config.QueueName = keyValue;
                }
                keyValue = nav.GetAttribute("Host", navigator.NamespaceURI);
                if (!string.IsNullOrEmpty(keyValue))
                {
                    config.Host = keyValue;
                }
                keyValue = nav.GetAttribute("Port", navigator.NamespaceURI);
                if (!string.IsNullOrEmpty(keyValue))
                {
                    config.Port = keyValue;
                }

                if (!config.IsEmpty())
                {
                    _dict.Add(config.QueueName, config);
                }
            }
            return _dict;
        }
    }
}
