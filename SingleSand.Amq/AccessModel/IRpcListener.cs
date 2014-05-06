using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.AccessModel
{
    public interface IRpcListener : IDisposable
    {
        string QueueName { get; }
        Task<ICollection<Message>> Receive(long conversationId, ReceiveArgs args);
        long GetNextConversationId();
    }
}