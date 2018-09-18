using System.Collections.Generic;
using System.Threading;

namespace Formix.Semaphore
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

        public abstract IEnumerable<Token> Tokens { get; }


        /// <summary>
        /// Wait until the given SemaphoreTask finish to execute. The task 
        /// starts only when there is enough Value in the semaphore to 
        /// execute the given task.
        /// </summary>
        /// <param name="token">The synchronization token used to wait 
        /// for the task.</param>
        /// <returns>An awaitable task.</returns>
        public void Wait(Token token)
        {
            Enqueue(token);
            while (!CanExecute(token))
            {
                Thread.Sleep(Delay);
            }
            token.IsRunning = true;
        }

        /// <summary>
        /// Signal back the usage of the semaphore that was consumed by the 
        /// executing task.
        /// </summary>
        /// <param name="token">The token used to reserve resources for the 
        /// task that finished running.</param>
        /// <returns>An awaitable task</returns>
        public void Signal(Token token)
        {
            lock (Tokens)
            {
                token.IsRunning = false;
                token.IsDone = true;
                Dequeue(token);
            }
        }

        /// <summary>
        /// Implements this method to add the semaphore task tou the 
        /// underlying queue used to keep the task list.
        /// </summary>
        /// <param name="semtask">The semaphore task to add to the 
        /// queue.</param>
        protected abstract void Enqueue(Token semtask);

        /// <summary>
        /// Implements this method to remove the semaphore task from the 
        /// underlying execution queue.
        /// </summary>
        /// <param name="semtask">The semaphore task to remove from the 
        /// queue.</param>
        protected abstract void Dequeue(Token semtask);

        /// <summary>
        /// Implements this method to return if a semaphore task can be 
        /// executed.
        /// </summary>
        /// <param name="semtask">The semaphore task that will be started if 
        /// true is returned.</param>
        protected abstract bool CanExecute(Token semtask);
    }
}
