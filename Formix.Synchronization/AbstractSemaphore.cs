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

        public async Task Execute(Action action, int usage = 1)
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
                await Wait(semtask);
            }
            finally
            {
                await Signal(semtask);
            }
        }

        /// <summary>
        /// Wait until the given SemaphoreTask finish to execute. The task 
        /// starts only when there is enough Value in the semaphore to 
        /// execute the given task.
        /// </summary>
        /// <param name="semtask">The semaphore task to wait for.</param>
        /// <returns>An awaitable task.</returns>
        protected virtual async Task Wait(SemaphoreTask semtask)
        {
            await Enqueue(semtask);
            while (!(await CanExecute(semtask)))
            {
                await Task.Delay(Delay);
            }
            semtask.Start();
            await semtask;
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
