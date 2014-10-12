using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleSand.Amq.AccessModel
{
    public static class RpcExtensions
    {
         public static async Task<ICollection<IMessage>> CallRemotely(this IPublisher publisher, IMessage message,
                                                                     IRpcListener listener, ReceiveArgs listenArgs)
         {
             var conversationId = listener.GetNextConversationId();
             message.ConversationId = conversationId;
             message.ResponseQueueName = listener.QueueName;
             var receiveTask = listener.Receive(conversationId, listenArgs);
             await publisher.Push(message);
             return await receiveTask;
         }
    }
}