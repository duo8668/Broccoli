using System;

namespace Broccoli.Core.Database.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SqlTableNameAttribute : Attribute
    {
        public readonly string Value;

        public SqlTableNameAttribute(string value)
        {
            this.Value = value;
        }
    }
}
