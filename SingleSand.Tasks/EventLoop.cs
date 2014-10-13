using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace SingleSand.Tasks
{
    public static class EventLoop
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Creates new event loop on current thread an runs <paramref name="mainTask"/> on it.
		/// </summary>
		/// <param name="mainTask">Initial task that has to execute in the event loop</param>
		/// <param name="stopAfterMainTask">True means that the loop has to stop after <paramref name="mainTask"/> finishes. False means
		/// that it does not stop after the <paramref name="mainTask"/> finishes and continues running
		/// until <see cref="SingleThreadSynchronizationContext.Cancel"/> method is called</param>
        public static void Run(Func<Task> mainTask, bool stopAfterMainTask)
        {
            var context = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);

            Func<Task> mainAsyncFunc =
                async () =>
                    {
                        try
                        {
                            await mainTask();
                        }
						catch (Exception exc)
						{
							Log.FatalException("Main task failed in event loop", exc);
							// in case of error in initial task it should be logged
							// because the error is not thrown till the event loop finishes,
							throw;
						}
						finally
                        {
							if (stopAfterMainTask)
								context.Cancel();
                        }
                    };
            var task = mainAsyncFunc(); //this will post the task to current SynchronizationContext

            Log.Info("Starting event loop on thread {0}", Thread.CurrentThread.ManagedThreadId);
            context.RunMessageLoop();
            Log.Info("Event loop stopped on thread {0}", Thread.CurrentThread.ManagedThreadId);

            task.Wait();//at this point the task is finished, this call will throw errors if any
        }
    }
}