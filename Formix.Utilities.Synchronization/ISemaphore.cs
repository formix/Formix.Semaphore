using System;
using System.Threading.Tasks;

namespace Formix.Utilities.Synchronization
{
    public interface ISemaphore
    {
        string Name { get; }
        int Quantity { get; }

        Task<bool> Execute(Action action, int usage = 1, int maxWaitTime = 0);
    }
}