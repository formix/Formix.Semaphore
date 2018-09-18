using System;

namespace Formix.Semaphore
{
    /// <summary>
    /// Represents a method to be executed by a Semaphore.
    /// </summary>
    public class Token : IComparable<Token>
    {
        /// <summary>
        /// Creates a SemaphoreTask with a given action to execute and a 
        /// usage value.
        /// </summary>
        /// <param name="usage">A value representing an amount of resource 
        /// consumed by the curent token. That usage value is substracted 
        /// from the semaphore while the task is executing and returned 
        /// back to the semaphore when the task is done.</param>
        /// <remarks>Usage must be grater than zero.</remarks>
        public Token(int usage = 1)
        {
            if (usage <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(usage), $"The argument must be grater than 0.");
            }

            Id = Guid.NewGuid();
            Usage = usage;
            IsRunning = false;
            IsDone = false;
            TimeStamp = DateTime.Now.Ticks;
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
        /// Gets if the current token is new and unused.
        /// </summary>
        public bool IsNew => !IsRunning && !IsDone;

        /// <summary>
        /// Gets if the task associated with the current token is running.
        /// </summary>
        public bool IsRunning { get; internal set; }

        /// <summary>
        /// Gets if the task associated with the current token is done executing.
        /// </summary>
        public bool IsDone { get; internal set; }

        /// <summary>
        /// Gets when the token was created (DateTime.Ticks).
        /// </summary>
        public long TimeStamp { get; }

        public int CompareTo(Token other)
        {
            var diff = TimeStamp - other.TimeStamp;
            if (diff < 0)
            {
                return -1;
            }

            if (diff > 0)
            {
                return 1;
            }

            return 0;
        }
    }
}
