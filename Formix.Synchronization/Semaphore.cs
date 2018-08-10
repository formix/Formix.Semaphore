using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formix.Synchronization
{
    public class Semaphore : AbstractSemaphore
    {
        #region static members
        private static readonly IDictionary<string, Semaphore> _semaphores;

        static Semaphore()
        {
            _semaphores = new Dictionary<string, Semaphore>();
        }


        public static Semaphore Initialize()
        {
            return Initialize("$mutex", 1);
        }

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


        private LinkedList<SemaphoreTask> _semaphoreTasks;


        private Semaphore(string name, int value)
        {
            Name = name;
            Value = value;
            _semaphoreTasks = new LinkedList<SemaphoreTask>();
        }

        public int TotalTaskCount
        {
            get
            {
                lock (_semaphoreTasks)
                {
                    return _semaphoreTasks.Count;
                }
            }
        }

        public int RunningTaskCount
        {
            get
            {
                lock (_semaphoreTasks)
                {
                    return _semaphoreTasks
                        .Where(t => t.IsRuning)
                        .Count();
                }
            }
        }

        public int RunningTaskUsage
        {
            get
            {
                lock (_semaphoreTasks)
                {
                    return _semaphoreTasks
                        .Where(t => t.IsRuning)
                        .Sum(t => t.Usage);
                }
            }
        }


        protected override async Task Enqueue(SemaphoreTask stask)
        {
            lock (_semaphoreTasks)
            {
                _semaphoreTasks.AddLast(stask);
            }
            await Task.CompletedTask;
        }

        protected override async Task Dequeue(SemaphoreTask stask)
        {
            await Task.Run(() =>
            {
                lock (_semaphoreTasks)
                {
                    _semaphoreTasks.Remove(stask);
                }
            });
        }

        protected override async Task<bool> CanExecute(SemaphoreTask stask)
        {
            if (stask.IsRuning)
            {
                throw new InvalidOperationException(
                    $"The semaphore task {stask.Id} is already running!");
            }

            return await Task.Run(() =>
            {
                lock (_semaphoreTasks)
                {
                    int remains = Value;
                    foreach (var e in _semaphoreTasks)
                    {
                        if (e == stask && remains >= stask.Usage)
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
            });
        }
    }
}
