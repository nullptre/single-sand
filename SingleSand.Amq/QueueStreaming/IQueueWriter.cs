using System;
using System.Threading.Tasks;
using SingleSand.Amq.AccessModel;

namespace SingleSand.Amq.QueueStreaming
{
    public interface IQueueWriter : IDisposable
    {
        Task Send(IMessage message);
    }
}