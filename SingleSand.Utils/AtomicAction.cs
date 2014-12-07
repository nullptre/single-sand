using System;
using System.Threading.Tasks;

namespace SingleSand.Utils
{
    /// <summary>
    /// Atomic action queues each new given task so that all taksk in the queue
    /// are executed sequentially (the next task executes after the previous ends)
    /// and they don't overlap.
    /// TODO check if it can be replaced with Task.ContinueWith()
    /// </summary>
    public class AtomicAction
    {
        private Task _pending;

        public async Task Run(Func<Task> atomic)
        {
            var initial = _pending;
            _pending = RunInternal(atomic, initial);
            await _pending;
        }

        private static async Task RunInternal(Func<Task> atomic, Task initial)
        {
            if (initial != null && !initial.IsCompleted)
                await Task.WhenAny(initial); //this will also skip errors in the initial task
            await atomic();
        }
    }
}