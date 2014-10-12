using System;
using System.Collections.Generic;
using SingleSand.Amq.AccessModel;

namespace SingleSand.Amq.Sync
{
    internal class ConversationAsyncResult : ManualResetAsyncResult
    {
        private readonly Func<ICollection<IMessage>, bool> _completionPredicate;

        public ConversationAsyncResult(Func<ICollection<IMessage>, bool> completionPredicate)
        {
            _completionPredicate = completionPredicate;
            Messages = new LinkedList<IMessage>();
        }

        public ICollection<IMessage> Messages { get; private set; }

        public bool Put(IMessage message)
        {
            Messages.Add(message);
            return _completionPredicate(Messages);
        }

        public void Complete()
        {
            this.CompleteInternal();
        }
    }
}