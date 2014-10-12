using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingleSand.Utils
{
    /// <summary>
    /// This class contains workarounds for lacks of pure async operations in some core classes
    /// </summary>
    public static class AsyncUtils
    {
        private static Task InfiniteTask(CancellationToken cancellation)
        {
			return Task.Delay(Timeout.Infinite, cancellation);
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