namespace Broccoli.Core.Configuration
{
    public class TaskQueueConfig
    {
        public string QueueName { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(QueueName) || string.IsNullOrEmpty(Host) || string.IsNullOrEmpty(Port);
        }
    }
}
