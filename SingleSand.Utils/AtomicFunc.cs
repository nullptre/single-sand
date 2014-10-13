using System;
using System.Threading.Tasks;

namespace SingleSand.Utils
{
	/// <summary>
	/// Atomic function queues each new given task so that all taksk in the queue
	/// are executed sequentially (the next task executes after the previous ends)
	/// and they don't overlap.
	/// TODO check if it can be replaced with Task.ContinueWith()
	/// </summary>
	/// <typeparam name="T"></typeparam>
    public class AtomicFunc<T>
    {
        private Task<T> _pending;

        public async Task<T> Run(Func<Task<T>> atomic)
        {
            var initial = _pending;
            _pending = RunInternal(atomic, initial);
            return await _pending;
        }

        private static async Task<T> RunInternal(Func<Task<T>> atomic, Task initial)
        {
            if (initial != null && !initial.IsCompleted)
                await Task.WhenAny(initial);//this will also skip errors in the initial task
            return await atomic();
        }
    }
}