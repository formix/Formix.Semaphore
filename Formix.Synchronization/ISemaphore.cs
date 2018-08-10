using System;
using System.Threading.Tasks;

namespace Formix.Synchronization
{
    public interface ISemaphore
    {
        string Name { get; }
        int Value { get; }
        int Delay { get; set; }

        Task<bool> Execute(Action action, int usage = 1, int maxWaitTime = 0);
    }
}