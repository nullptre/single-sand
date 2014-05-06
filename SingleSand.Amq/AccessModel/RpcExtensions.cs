using System.Collections.Generic;
using System.Threading.Tasks;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.AccessModel
{
    public static class RpcExtensions
    {
         public static async Task<ICollection<Message>> CallRemotely(this IPublisher publisher, Message message,
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