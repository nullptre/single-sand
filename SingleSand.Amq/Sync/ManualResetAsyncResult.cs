using System;
using System.Threading;

namespace SingleSand.Amq.Sync
{
    /// <summary>
    /// Used for asynchronous operations
    /// </summary>
    internal abstract class ManualResetAsyncResult : IAsyncResult, IDisposable
    {
        private readonly ManualResetEvent _event = new ManualResetEvent(false);

        #region IAsyncResult Members

        public bool IsCompleted
        {
            get { return this._event.WaitOne(0); }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return this._event; }
        }

        public object AsyncState
        {
            get { return null; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this._event.Dispose();
        }

        #endregion

        protected void CompleteInternal()
        {
            this._event.Set();
        }
    }
}