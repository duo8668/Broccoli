using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConnectionStringAttribute : Attribute
    {
        public readonly string Value;

        public ConnectionStringAttribute(string value)
        {
            this.Value = value;
        }
    }
}
