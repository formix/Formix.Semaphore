using System;
using System.Threading.Tasks;

namespace Formix.Synchronization
{
    /// <summary>
    /// Represents a method to be executed by a Semaphore.
    /// </summary>
    public class SemaphoreTask
    {
        private readonly Action _action;

        /// <summary>
        /// Creates a SemaphoreTask with a given action to execute and a 
        /// usage value.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="usage">A value representing an amount of resource 
        /// consumed by the curent action. That usage value is substracted 
        /// from the semaphore while the task is executing and returned 
        /// back to the semaphore when the task is done.</param>
        /// <remarks>Usage must be grater than zero.</remarks>
        public SemaphoreTask(Action action, int usage)
        {
            if (usage <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(usage), $"The argument must be grater than 0.");
            }

            _action = action ?? throw new ArgumentNullException(nameof(action));
            Id = Guid.NewGuid();
            Usage = usage;
            IsRuning = false;
            IsDone = false;
        }

        /// <summary>
        /// A unique identifier for the current task.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The amount of resources consumed by the current task.
        /// </summary>
        public int Usage { get; }

        /// <summary>
        /// Gets if the current task is actually runnung.
        /// </summary>
        public bool IsRuning { get; private set; }

        /// <summary>
        /// Gets if the current task execution is done.
        /// </summary>
        public bool IsDone { get; private set; }


        public async Task Wait(int delay = 5)
        {
            var start = DateTime.Now;
            var delayPeriod = TimeSpan.FromMilliseconds(delay);
            while (!IsDone || 
                    ((delay > 0) && (DateTime.Now - start < delayPeriod)))
            {
                await Task.Delay(delay);
            }
        }

        internal async Task Execute()
        {
            if (IsRuning)
            {
                throw new InvalidOperationException(
                    "The task is already running.");
            }
            if (IsDone)
            {
                throw new InvalidOperationException(
                    "The task already ran. You cannot execute it again");
            }

            IsRuning = true;
            try
            {
                await Task.Run(_action);
            }
            finally
            {
                IsRuning = false;
                IsDone = true;
            }
        }
    }
}
