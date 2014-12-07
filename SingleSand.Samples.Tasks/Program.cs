using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using SingleSand.Tasks;

namespace SingleSand.Samples.Tasks
{
    /// <summary>
    /// This code demonstrates how event loop works in siple console application
    /// </summary>
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly ICollection<Task> _pendingIoOperations = new List<Task>();

        static void Main(string[] args)
        {
            Log.Info("Starting App");

            //start an event loop on current thread and 
            EventLoop.Run(MainMain, true);
            Log.Info("App exit");
        }

        private static async Task MainMain()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                Log.Info("I am inside the event loop!");

                try
                {
                    ConsoleKey key;
                    do
                    {
                        key = await Task.Run(() => Console.ReadKey().Key, cancellation.Token);

                        if (key == ConsoleKey.Q)
                        {
                            Log.Info("Quit requested");
                            continue;
                        }

                        if (key == ConsoleKey.D)
                            //run asynchronous IO and wait for it
                            await DoSomeIoStuff(cancellation.Token);

                        if (key == ConsoleKey.S)
                        {
                            //start asynchronous IO without waiting for completion
                            var newTask = DoSomeIoStuff(cancellation.Token);
                            _pendingIoOperations.Add(newTask);
                            newTask.ContinueWith(t => _pendingIoOperations.Remove(t), cancellation.Token);
                        }
                    }
                    while (key != ConsoleKey.Q);
                }
                finally
                {
                    cancellation.Cancel();
                }

                await Task.WhenAll(_pendingIoOperations);
            }
        }

        private static async Task DoSomeIoStuff(CancellationToken cancellation)
        {
            Log.Info("Starting async IO operation...");
            //simulate the operation
            await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(3), cancellation));
            Log.Info("Async IO operation completed.");
        }
    }
}
