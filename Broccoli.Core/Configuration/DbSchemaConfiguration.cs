using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Broccoli.Core.Configuration
{
    public class DbSchemaConfiguration
    {
        public static Dictionary<string, ModelSchemaConfig> _configs;

        public static Dictionary<string, ModelSchemaConfig> Configs
        {
            get
            {
                if (_configs == null)
                {
                    _configs = Deserialize("ModelSchema.config");
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
        public static Dictionary<string, ModelSchemaConfig> Deserialize(string file)
        {
            Dictionary<string, ModelSchemaConfig> _dict = new Dictionary<string, ModelSchemaConfig>();
            XmlReader reader = XmlReader.Create(file);
            XPathDocument document = new XPathDocument(reader);
            XPathNavigator navigator = document.CreateNavigator();

            foreach (XPathNavigator nav in navigator.Select("/*/Classes/Class"))
            {
                var config = new ModelSchemaConfig();

                string keyValue = nav.GetAttribute("Name", navigator.NamespaceURI);
                if (!string.IsNullOrEmpty(keyValue))
                {
                    config.Name = keyValue;
                }
                keyValue = nav.GetAttribute("DatabaseConnectionName", navigator.NamespaceURI);
                if (!string.IsNullOrEmpty(keyValue))
                {
                    config.DatabaseConnectionName = keyValue;
                }
                keyValue = nav.GetAttribute("TableName", navigator.NamespaceURI);
                if (!string.IsNullOrEmpty(keyValue))
                {
                    config.TableName = keyValue;
                }

                if (!config.IsEmpty())
                {
                    _dict.Add(config.Name, config);
                }
            }
            return _dict;
        }
    }
}
