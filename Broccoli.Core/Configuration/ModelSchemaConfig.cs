namespace Broccoli.Core.Configuration
{
    public class ModelSchemaConfig
    {
        public string Name { get; set; }
        public string DatabaseConnectionName { get; set; }
        public string TableName { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(DatabaseConnectionName) || string.IsNullOrEmpty(TableName);
        }
    }
}
