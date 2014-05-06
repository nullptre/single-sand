using System;
using System.Collections.Generic;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.Sync
{
    internal class ConversationAsyncResult : ManualResetAsyncResult
    {
        private readonly Func<ICollection<Message>, bool> _completionPredicate;

        public ConversationAsyncResult(Func<ICollection<Message>, bool> completionPredicate)
        {
            _completionPredicate = completionPredicate;
            Messages = new LinkedList<Message>();
        }

        public ICollection<Message> Messages { get; private set; }

        public bool Put(Message message)
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