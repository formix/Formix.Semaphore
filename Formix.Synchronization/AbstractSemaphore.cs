using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Formix.Synchronization
{
    /// <summary>
    /// Base implementation of a Semaphore. Abstract away Wait and Signal 
    /// methods and allows a seamless execution guaranteed to be within the 
    /// limits specified by the semaphore value.
    /// </summary>
    public abstract class AbstractSemaphore : ISemaphore
    {
        /// <summary>
        /// Creates a Semaphore with a default delay set to 5ms between 
        /// CanExecute checks. This default value shall be changed based 
        /// on the implementation.
        /// </summary>
        public AbstractSemaphore() => Delay = 5;


        public string Name { get; protected set; }

        public int Value { get; protected set; }

        public int Delay { get; set; }

        public abstract IEnumerable<SemaphoreTask> Tasks { get; }

        public async Task<SemaphoreTask> Execute(Action action, int usage = 1, int maxWaitTime = 0)
        {
            if (usage > Value)
            {
                throw new ArgumentException(
                    $"Can not use {usage} {Name} from that semaphore. The " +
                        $"semaphore initial value is {Value}.",
                    nameof(usage));
            }

            var semtask = new SemaphoreTask(action, usage);
            try
            {
                var taskExecuted = await Wait(semtask, maxWaitTime);
                if (!taskExecuted)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    // Start another task out of the awaitable chain
                    Task.Run(async () => 
                    {
                        try
                        {
                            await WaitWithoutEnqueue(semtask, 0);
                        }
                        finally
                        {
                            await Signal(semtask);
                        }
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                return semtask;
            }
            finally
            {
                await Signal(semtask);
            }
        }

        /// <summary>
        /// Wait until the given SemaphoreTask finish to execute or return 
        /// before the maxWaitTime is expired, given that the SemaphoreTask 
        /// is not started. The task starts only when there is enough Value 
        /// in the semaphore to execute the given task.
        /// </summary>
        /// <param name="semtask">The semaphore task to wait for.</param>
        /// <param name="maxWaitTime">How long in milliseconds shall the 
        /// wait function blocks before the SemaphoreTask starts. If this 
        /// value is reached before the task is started, returns false.</param>
        /// <returns>True if the SemaphoreTask executed, false if maxWaitTime 
        /// was reached and the SemaphoreTask did not execute.</returns>
        protected virtual async Task<bool> Wait(SemaphoreTask semtask, int maxWaitTime)
        {
            await Enqueue(semtask);
            return await WaitWithoutEnqueue(semtask, maxWaitTime);
        }

        private async Task<bool> WaitWithoutEnqueue(SemaphoreTask semtask, int maxWaitTime)
        {
            var endTime = DateTime.Now + TimeSpan.FromMilliseconds(maxWaitTime);
            while (!(await CanExecute(semtask)))
            {
                await Task.Delay(Delay);
                if (maxWaitTime > 0 && DateTime.Now > endTime)
                {
                    return false;
                }
            }
            await semtask.Execute();
            return true;
        }

        /// <summary>
        /// Signal back the usage of the semaphore that was consumed by the 
        /// executing task.
        /// </summary>
        /// <param name="semtask">The semaphore taks that ended its 
        /// execution.</param>
        /// <returns>An awaitable task</returns>
        protected virtual async Task Signal(SemaphoreTask semtask)
        {
            await Dequeue(semtask);
        }

        /// <summary>
        /// Implements this method to add the semaphore task tou the 
        /// underlying queue used to keep the task list.
        /// </summary>
        /// <param name="semtask">The semaphore task to add to the 
        /// queue.</param>
        /// <returns></returns>
        protected abstract Task Enqueue(SemaphoreTask semtask);

        /// <summary>
        /// Implements this method to remove the semaphore task from the 
        /// underlying execution queue.
        /// </summary>
        /// <param name="semtask">The semaphore task to remove from the 
        /// queue.</param>
        /// <returns>An awaitable task.</returns>
        protected abstract Task Dequeue(SemaphoreTask semtask);

        /// <summary>
        /// Implements this method to return if a semaphore task can be 
        /// executed.
        /// </summary>
        /// <param name="semtask">The semaphore task that will be started if 
        /// true is returned.</param>
        /// <returns>An awaitable task that results in tru if the given 
        /// SemaphoreTask is to be executed or false if the Wait method 
        /// shall wait for another Delai time (in ms) before asking 
        /// again.</returns>
        protected abstract Task<bool> CanExecute(SemaphoreTask semtask);
    }
}
