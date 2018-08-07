using System;
using System.Threading.Tasks;

namespace Formix.Utilities.Synchronization
{
    public abstract class AbstractSemaphore : ISemaphore
    {
        public string Name { get; protected set; }

        public int Quantity { get; protected set; }

        public async Task<bool> Execute(Action action, int usage = 1, int maxWaitTime = 0)
        {
            if (usage > Quantity)
            {
                throw new ArgumentException(
                    $"Can not use more than what is originally " +
                        $"available in the semaphore. The quantity of " +
                        $"{Name} available is {Quantity}.",
                    nameof(usage));
            }

            var entry = new SemaphoreTask(action, usage);

            try
            {
                return await Wait(entry, maxWaitTime);
            }
            finally
            {
                await Signal(entry);
            }
        }

        protected virtual async Task<bool> Wait(SemaphoreTask entry, int maxWaitTime)
        {
            await Enqueue(entry);
            var startTime = DateTime.Now;
            var maximumWaitTime = TimeSpan.FromMilliseconds(maxWaitTime);
            while (!(await CanExecute(entry)))
            {
                await Task.Delay(50);
                if (maxWaitTime > 0 &&
                        DateTime.Now - startTime > maximumWaitTime)
                {
                    return false;
                }
            }
            await entry.Execute();
            return true;
        }
        protected virtual async Task Signal(SemaphoreTask entry)
        {
            await Dequeue(entry);
        }

        protected abstract Task Enqueue(SemaphoreTask entry);
        protected abstract Task Dequeue(SemaphoreTask entry);

        protected abstract Task<bool> CanExecute(SemaphoreTask entry);
    }
}
