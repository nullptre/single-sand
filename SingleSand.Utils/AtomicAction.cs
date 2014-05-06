using System;
using System.Threading.Tasks;

namespace SingleSand.Utils
{
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