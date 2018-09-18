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

        private ICollection<Token> _tasks;

        /// <summary>
        /// List of SemaphoreTasks that are queued.
        /// </summary>
        public override IEnumerable<Token> Tokens => _tasks;


        private Semaphore(string name, int value)
        {
            Name = name;
            Value = value;
            _tasks = new SortedSet<Token>();
        }

        /// <summary>
        /// Enqueues a new semaphore task to be executed once enough 
        /// resources (Semaphore.Value) are available.
        /// </summary>
        /// <param name="token">A semaphore task to execute.</param>
        /// <returns>An awaitable task.</returns>
        protected override void Enqueue(Token token)
        {
            lock (_tasks)
            {
                _tasks.Add(token);
            }
        }

        /// <summary>
        /// Removes a task from the head section of the queue.
        /// </summary>
        /// <param name="token">The semaphore task to remove.</param>
        /// <returns>An awaitable task.</returns>
        /// <remarks>The task removed may not be the task at the head of the 
        /// queue. It is possible that a task deeper in the "head" section 
        /// terminated before and thus needs to be removed.</remarks>
        protected override void Dequeue(Token token)
        {
            lock (_tasks)
            {
                _tasks.Remove(token);
            }
        }
    }
}
