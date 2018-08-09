using System;
using System.Threading.Tasks;

namespace Formix.Utilities.Synchronization
{
    public abstract class AbstractSemaphore : ISemaphore
    {
        public AbstractSemaphore() => Delay = 50;


        public string Name { get; protected set; }
        public int Value { get; protected set; }
        public int Delay { get; set; }

        public async Task<bool> Execute(Action action, int usage = 1, int maxWaitTime = 0)
        {
            if (usage > Value)
            {
                throw new ArgumentException(
                    $"Can not use {usage} {Name} from that semaphore. The " +
                        $"semaphore initial value is {Value}.",
                    nameof(usage));
            }

            var stask = new SemaphoreTask(action, usage);
            try
            {
                return await Wait(stask, maxWaitTime);
            }
            finally
            {
                await Signal(stask);
            }
        }

        protected virtual async Task<bool> Wait(SemaphoreTask stask, int maxWaitTime)
        {
            await Enqueue(stask);
            var endTime = DateTime.Now + TimeSpan.FromMilliseconds(maxWaitTime);
            while (!(await CanExecute(stask)))
            {
                await Task.Delay(Delay);
                if (maxWaitTime > 0 && DateTime.Now > endTime)
                {
                    return false;
                }
            }
            await stask.Execute();
            return true;
        }
        protected virtual async Task Signal(SemaphoreTask stask)
        {
            await Dequeue(stask);
        }

        protected abstract Task Enqueue(SemaphoreTask stask);
        protected abstract Task Dequeue(SemaphoreTask stask);
        protected abstract Task<bool> CanExecute(SemaphoreTask stask);
    }
}
