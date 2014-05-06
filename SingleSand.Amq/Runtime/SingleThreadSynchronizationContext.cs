using System;
using System.Collections.Generic;
using System.Threading;

namespace SingleSand.Amq.Runtime
{
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly Queue<Action> _messagesToProcess = new Queue<Action>();
        private readonly Thread _mainThread;
        private readonly object _syncHandle = new object();

        private readonly ManualResetEvent _isStoppedEvent = new ManualResetEvent(false);
        private readonly AutoResetEvent _isNewMessageEvent = new AutoResetEvent(false);

        public SingleThreadSynchronizationContext()
        {
            _mainThread = Thread.CurrentThread;
        }

        public override void Send(SendOrPostCallback workItem, object state)
        {
            var thisWorkItemResultEvent = new ManualResetEventSlim();
            Post(s =>
                {
                    try
                    {
                        workItem(s);
                    }
                    finally
                    {
                        thisWorkItemResultEvent.Set();
                    }
                },
                state);

            if (Thread.CurrentThread == _mainThread)
            {
                RunMessagesChain(thisWorkItemResultEvent);
            }
            thisWorkItemResultEvent.Wait();
        }

        public override void Post(SendOrPostCallback workItem, object state)
        {
            lock (_syncHandle)
            {
                _messagesToProcess.Enqueue(() => workItem(state));
            }
            SignalNewMessage();
        }

        private void RunMessagesChain(ManualResetEventSlim completionSignal)
        {
            if (Thread.CurrentThread != _mainThread)
                throw new InvalidOperationException("Message loop cannot run on thread other than this Synchronization context was created on");

            Action nextWorkItem;
            while (
                (completionSignal == null || !completionSignal.IsSet)
                    && !_isStoppedEvent.WaitOne(0)
                    && (nextWorkItem = PopItem()) != null)
            {
                nextWorkItem();
            }
        }

        public void RunMessageLoop()
        {
            while (WaitHandle.WaitAny(new WaitHandle[] { _isNewMessageEvent, _isStoppedEvent }) == 0)
            {
                RunMessagesChain(null);
            }
        }

        private Action PopItem()
        {
            lock (_syncHandle)
            {
                return _messagesToProcess.Count > 0
                    ? _messagesToProcess.Dequeue()
                    : null;
            }
        }

        public void Cancel()
        {
            _isStoppedEvent.Set();
        }

        private void SignalNewMessage()
        {
            _isNewMessageEvent.Set();
        }
    }
}