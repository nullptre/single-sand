using System;
using System.Collections.Generic;
using System.Threading;

namespace SingleSand.Amq.AccessModel
{
    public class ReceiveArgs
    {
        public Func<ICollection<IMessage>, bool> CompletionPredicate { get; private set; }
        public TimeSpan Timeout { get; private set; }
        public CancellationToken Cancellation { get; private set; }

        public ReceiveArgs(Func<ICollection<IMessage>, bool> completionPredicate, TimeSpan timeout,
                       CancellationToken cancellation)
        {
            this.CompletionPredicate = completionPredicate;
            this.Timeout = timeout;
            this.Cancellation = cancellation;
        }
    }
}