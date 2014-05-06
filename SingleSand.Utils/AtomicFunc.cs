using System;
using System.Threading.Tasks;

namespace SingleSand.Utils
{
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