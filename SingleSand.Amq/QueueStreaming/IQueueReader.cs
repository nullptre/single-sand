using System;
using System.Threading.Tasks;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.QueueStreaming
{
    public interface IQueueReader : IDisposable
    {
        /// <summary>
        /// This event occurs in single thread, so it guarantees
        /// that no parallel handling will execute
        /// </summary>
        event Func<Message, Task> NewMessage;

        string QueueName { get; }
    }
}