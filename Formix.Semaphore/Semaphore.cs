using System;
using System.Collections.Generic;

namespace Formix.Semaphore
{
    /// <summary>
    /// In-process implementation of the AbstractSemaphore.
    /// </summary>
    public class Semaphore : AbstractSemaphore
    {
        #region static members
        private static readonly IDictionary<string, Semaphore> _semaphores;

        static Semaphore()
        {
            _semaphores = new Dictionary<string, Semaphore>();
        }

        /// <summary>
        /// Creates a global semaphore that behaves like a simple mutex.
        /// </summary>
        /// <returns>A semaphore called '$mutex' with a value of 1.</returns>
        public static Semaphore Initialize()
        {
            return Initialize("$mutex", 1);
        }

        /// <summary>
        /// Initialize a semaphore in the global context with the given name 
        /// and value. If a semaphore exists with the same name and value, 
        /// that semaphore instance will be returned instead of creating a 
        /// new one.
        /// </summary>
        /// <param name="name">The name of the semaphore to initialize.</param>
        /// <param name="value">The value given to the semaphore.</param>
        /// <returns>A semaphore that can pile-up and execute tasks within the 
        /// given semaphore value range.</returns>
        /// <remarks>Initializing multiple semaphore bearing the same name 
        /// but with different values will throw an 
        /// InvalidOperationException.</remarks>
        public static Semaphore Initialize(string name, int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), "The argument must be grater than 0.");
            }

            lock (_semaphores)
            {
                if (!_semaphores.ContainsKey(name))
                {
                    _semaphores.Add(name,
                        new Semaphore(name, value));
                }

                var semaphore = _semaphores[name];
                if (semaphore.Value != value)
                {
                    throw new InvalidOperationException(
                        $"You cannot initialize the semaphore {name} " +
                            $"with a value of {value}. That " +
                            $"semaphore already exist with a different " +
                            $"value: {semaphore.Value}.");
                }

                return semaphore;
            }
        }
        #endregion

        private ICollection<Token> _semaphoreTasks;

        /// <summary>
        /// List of SemaphoreTasks that are queued.
        /// </summary>
        public override IEnumerable<Token> Tokens => _semaphoreTasks;


        private Semaphore(string name, int value)
        {
            Name = name;
            Value = value;
            _semaphoreTasks = new SortedSet<Token>();
        }

        /// <summary>
        /// Enqueues a new semaphore task to be executed once enough 
        /// resources (Semaphore.Value) are available.
        /// </summary>
        /// <param name="semtask">A semaphore task to execute.</param>
        /// <returns>An awaitable task.</returns>
        protected override void Enqueue(Token semtask)
        {
            lock (_semaphoreTasks)
            {
                _semaphoreTasks.Add(semtask);
            }
        }

        /// <summary>
        /// Removes a task from the head section of the queue.
        /// </summary>
        /// <param name="semtask">The semaphore task to remove.</param>
        /// <returns>An awaitable task.</returns>
        /// <remarks>The task removed may not be the task at the head of the 
        /// queue. It is possible that a task deeper in the "head" section 
        /// terminated before and thus needs to be removed.</remarks>
        protected override void Dequeue(Token semtask)
        {
            lock (_semaphoreTasks)
            {
                _semaphoreTasks.Remove(semtask);
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
        protected override bool CanExecute(Token token)
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

            lock (_semaphoreTasks)
            {
                int remains = Value;
                foreach (var e in _semaphoreTasks)
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
    }
}
