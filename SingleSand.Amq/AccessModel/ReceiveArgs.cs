using System;
using System.Collections.Generic;
using System.Threading;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.AccessModel
{
    public class ReceiveArgs
    {
        public Func<ICollection<Message>, bool> CompletionPredicate { get; private set; }
        public TimeSpan Timeout { get; private set; }
        public CancellationToken Cancellation { get; private set; }

        public ReceiveArgs(Func<ICollection<Message>, bool> completionPredicate, TimeSpan timeout,
                       CancellationToken cancellation)
        {
            this.CompletionPredicate = completionPredicate;
            this.Timeout = timeout;
            this.Cancellation = cancellation;
        }
    }
}