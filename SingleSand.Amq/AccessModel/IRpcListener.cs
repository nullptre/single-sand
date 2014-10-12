using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleSand.Amq.AccessModel
{
    public interface IRpcListener : IDisposable
    {
        string QueueName { get; }
        Task<ICollection<IMessage>> Receive(long conversationId, ReceiveArgs args);
        long GetNextConversationId();
    }
}