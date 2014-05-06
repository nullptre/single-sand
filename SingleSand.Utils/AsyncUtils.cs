using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SingleSand.Utils
{
    /// <summary>
    /// This class contains workarounds for lacks of pure async operations in some core classes
    /// </summary>
    public static class AsyncUtils
    {
        private static readonly ConditionalWeakTable<WaitHandle, Task> InfiniteTasksCache
            = new ConditionalWeakTable<WaitHandle, Task>();

        private static async Task InfiniteTask(CancellationToken cancellation)
        {
            Task result;
            if (!InfiniteTasksCache.TryGetValue(cancellation.WaitHandle, out result))
            {
                result = StartInfiniteTask(cancellation);
                //key is WaitHandle, since it is unique for each cancellation source
                InfiniteTasksCache.Add(cancellation.WaitHandle, result);
            }
            await result;
        }

        private static async Task StartInfiniteTask(CancellationToken cancellation)
        {
            //TODO use more elegant way of infinite task
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(100000, cancellation);
            }
        }

        public static async Task<T> CancelWith<T>(this Task<T> operation, Func<T> cancellationResult, CancellationToken cancellation)
        {
            return await await Task.WhenAny(operation, InfiniteTask(cancellation).ContinueWith(t => cancellationResult()));
        }

        public static async Task CancelWith(this Task operation, Action cancellationResult, CancellationToken cancellation)
        {
            await await Task.WhenAny(operation, InfiniteTask(cancellation).ContinueWith(t => cancellationResult()));
        }
    }
}