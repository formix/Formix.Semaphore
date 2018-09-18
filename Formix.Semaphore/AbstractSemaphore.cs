using System;
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
        /// Checks if the remaining resources available 
        /// (Semaphore.Value - sum of running tasks usage) are enough to 
        /// start the current task.
        /// </summary>
        /// <param name="token">The semaphore task that we are checking.</param>
        /// <returns>An awaitable task that will result in true if the 
        /// SemaphoreTask.Usage is less or equal to the remaining resources 
        /// or false otherwise.</returns>
        protected bool CanExecute(Token token)
        {
            if (token.IsRunning)
            {
                throw new InvalidOperationException(
                    $"The semaphore task associated with the token " +
                    $"{token.Id} is already running!");
            }

            if (token.IsDone)
            {
                throw new InvalidOperationException(
                    $"The semaphore task associated with the token " +
                    $"{token.Id} is is done executing. Create another " +
                    $"token to overlook another task.");
            }

            lock (Tokens)
            {
                int remains = Value;
                foreach (var e in Tokens)
                {
                    if (e == token && remains >= token.Usage)
                    {
                        return true;
                    }

                    remains -= e.Usage;
                    if (remains <= 0)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Implements this method to add the semaphore task tou the 
        /// underlying queue used to keep the task list.
        /// </summary>
        /// <param name="token">The semaphore task to add to the 
        /// queue.</param>
        protected abstract void Enqueue(Token token);

        /// <summary>
        /// Implements this method to remove the semaphore task from the 
        /// underlying execution queue.
        /// </summary>
        /// <param name="token">The semaphore task to remove from the 
        /// queue.</param>
        protected abstract void Dequeue(Token token);

    }
}
