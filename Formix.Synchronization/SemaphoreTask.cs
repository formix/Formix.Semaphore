using System;
using System.Threading.Tasks;

namespace Formix.Synchronization
{
    public class SemaphoreTask
    {
        private readonly Action _action;

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
        }

        public Guid Id { get; }
        public int Usage { get; }
        public bool IsRuning { get; private set; }

        public async Task Execute()
        {
            IsRuning = true;
            try
            {
                await Task.Run(_action);
            }
            finally
            {
                IsRuning = false;
            }
        }
    }
}
