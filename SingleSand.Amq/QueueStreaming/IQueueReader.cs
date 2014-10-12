using System;
using System.Threading.Tasks;
using SingleSand.Amq.AccessModel;

namespace SingleSand.Amq.QueueStreaming
{
    public interface IQueueReader : IDisposable
    {
        /// <summary>
        /// This event occurs in single thread, so it guarantees
        /// that no parallel handling will execute
        /// </summary>
        event Func<IMessage, Task> NewMessage;

        string QueueName { get; }
    }
}