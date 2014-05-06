using System;

namespace SingleSand.Amq.QueueStreaming
{
    public interface IQueueWriterFactory : IDisposable
    {
        IQueueWriter Get(string queueName);
    }
}