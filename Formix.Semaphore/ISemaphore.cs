using System.Collections.Generic;

namespace Formix.Semaphore
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
        IEnumerable<Token> Tokens { get; }

        /// <summary>
        /// Wait for resources. Blocks until the semaphore have enough 
        /// resource to execute the task.
        /// </summary>
        /// <param name="token">The token to wait for.</param>
        void Wait(Token token);

        /// <summary>
        /// Releases the resources used by the token.
        /// </summary>
        /// <param name="token">The token used to reserve resources.</param>
        void Signal(Token token);
    }
}