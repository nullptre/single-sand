using System;

namespace SingleSand.Amq.QueueStreaming
{
    public interface IQueueReaderFactory : IDisposable
    {
        IQueueReader Get(string queueName);
    }
}