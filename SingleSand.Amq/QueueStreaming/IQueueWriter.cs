using System;
using System.Threading.Tasks;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.QueueStreaming
{
    public interface IQueueWriter : IDisposable
    {
        Task Send(Message message);
    }
}