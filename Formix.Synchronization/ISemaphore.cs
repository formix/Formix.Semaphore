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
        /// Executes the given action with an optional usage and maxWaitTime.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="usage">How much of the Semaphore.Value does that 
        /// action consumes while executing. Must be grater than zero and 
        /// lower or equal to the Semaphore.Value value.</param>
        /// <param name="maxWaitTime">How long shall the method block while 
        /// the semaphore waits before the action begins to execute.</param>
        /// <returns>A task that will result in a SemaphoreTask</returns>
        /// <remarks>If the action starts executing before the maxWaitTime 
        /// value is reached and the wait time + execution time takes 
        /// longer than this value, the execution of the action will 
        /// terminate without being interrupted.</remarks>
        Task<SemaphoreTask> Execute(Action action, int usage = 1, int maxWaitTime = 0);
    }
}