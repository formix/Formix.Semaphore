using System;
using System.Threading.Tasks;

namespace Formix.Utilities.Synchronization
{
    public class SemaphoreTask
    {
        private readonly Action _action;

        public SemaphoreTask(Action action, int usage)
        {
            _action = action;
            Id = Guid.NewGuid();
            Usage = usage;
            IsRuning = false;
        }

        public Guid Id { get; private set; }
        public int Usage { get; private set; }
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
