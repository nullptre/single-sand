using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace SingleSand.Amq.Runtime
{
    public static class Utils
    {
        private static readonly Logger Log = LogManager.GetLogger("Runtime");

        public static void Run(Func<Task> main)
        {
            var context = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);

            Func<Task> mainAsyncFunc =
                async () =>
                    {
                        try
                        {
                            await main();
                        }
                        finally
                        {
                            context.Cancel();
                        }
                    };
            var task = mainAsyncFunc();//this will post the task to current SynchronizationContext

            Log.Info("Starting message loop on thread {0}", Thread.CurrentThread.ManagedThreadId);
            context.RunMessageLoop();
            Log.Info("Message loop stopped on thread {0}", Thread.CurrentThread.ManagedThreadId);

            task.Wait();//at this point the task is finished, this call will throw errors if any
        }
    }
}