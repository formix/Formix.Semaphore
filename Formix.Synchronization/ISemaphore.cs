using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Formix.Synchronization
{
    /// <summary>
    /// Interface defining core Semaphore features.
    /// </summary>
    public interface ISemaphore
    {
        /// <summary>
        /// The name of the Semaphore.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The value of the semaphore represents the total amount of 
        /// resources that can be consumed by the execution of a set of
        /// SemaphoreTasks. The sum of running SemaphoreTask.Usage must 
        /// aslways be lower or equal to this value.
        /// </summary>
        int Value { get; }

        /// <summary>
        /// Gets the waiting time (in milliseconds) a queued task shall wait 
        /// before check if it can run while it is queued.
        /// if it can execute.
        /// </summary>
        int Delay { get; set; }

        /// <summary>
        /// Gets an enumerator of all SemaphoreTasks that are currently 
        /// executing and queued in this semaphore.
        /// </summary>
        IEnumerable<SemaphoreTask> Tasks { get; }

        /// <summary>
        /// Executes the given action with an optional usage value.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="usage">How much of the Semaphore.Value does that 
        /// action consumes while executing. Must be grater than zero and 
        /// lower or equal to the Semaphore.Value value. Defaults to 1.</param>
        /// <returns>An awaitable task</returns>
        Task Execute(Action action, int usage = 1);
    }
}