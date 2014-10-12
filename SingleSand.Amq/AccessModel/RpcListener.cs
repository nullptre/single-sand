using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using SingleSand.Amq.QueueStreaming;
using SingleSand.Amq.Sync;

namespace SingleSand.Amq.AccessModel
{
    public class RpcListener : IRpcListener
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IDictionary<long, ConversationAsyncResult> _conversations
            = new Dictionary<long, ConversationAsyncResult>();

        private long _nextConversationId;

        private readonly IQueueReader _queueReader;

        public RpcListener(IQueueReader reader)
        {
            this._queueReader = reader;
            this._queueReader.NewMessage += this.OnNewMessageReceived;
        }

        public string QueueName
        {
            get { return _queueReader.QueueName; }
        }

        public async Task<ICollection<IMessage>> Receive(long conversationId, ReceiveArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");

            if (_conversations.ContainsKey(conversationId))
            {
                throw new InvalidOperationException(
                    string.Format("Conversation {0} already has a listener. Multiple listeners are not allowed.",
                                  conversationId));
            }
            var asncResult = new ConversationAsyncResult(args.CompletionPredicate);
            _conversations.Add(conversationId, asncResult);

            try
            {
                var messageTask = Task.Factory.FromAsync(asncResult, r => ((ConversationAsyncResult) r).Messages);
                var timeoutTask =
                    Task.Delay(args.Timeout, args.Cancellation).ContinueWith(t => (ICollection<IMessage>) null);
                messageTask = await Task.WhenAny(messageTask, timeoutTask);

                return await messageTask;
            }
            finally
            {
                if (!_conversations.Remove(conversationId))
                {
                    Log.Warn(string.Format("Conversation {0} is not present in result dictionary. Something went wrong.",
                                           conversationId));
                }
            }
        }

        public long GetNextConversationId()
        {
            return _nextConversationId++;
        }

        private async Task OnNewMessageReceived(IMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            if (!message.ConversationId.HasValue)
            {
                //messages out of conversations are ignored by this listener
                return;
            }

            ConversationAsyncResult conversation;
            if (!_conversations.TryGetValue(message.ConversationId.Value, out conversation) || conversation == null)
            {
                Log.Warn("Unexpected message got to {0}, no listeners of conversation {1}", this._queueReader, message.ConversationId);
                return;
            }

            if (conversation.Put(message))
                conversation.Complete();
        }

        public void Dispose()
        {
            this._queueReader.NewMessage -= this.OnNewMessageReceived;
        }
    }
}
