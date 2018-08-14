using System;
using System.Linq;
using System.Threading.Tasks;

namespace Formix.Semaphore.Tests.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var semaphore = ExecuteThreeTasks();
            Task.WaitAll(semaphore.Tasks.ToArray());
            Console.WriteLine("Waiting!!!");
            Task.Delay(2500).Wait();
        }





        private static Semaphore ExecuteThreeTasks()
        {
            var semaphore = Semaphore.Initialize("connections", 2);

            var task1 = semaphore.Execute(() =>
            {
                Console.WriteLine("Task 1 started.");
                Task.Delay(2500).Wait();
                Console.WriteLine("Task 1 done.");
            });

            var task2 = semaphore.Execute(() =>
            {
                Console.WriteLine("Task 2 started.");
                Task.Delay(5000).Wait();
                Console.WriteLine("Task 2 done.");
            });

            var task3 = semaphore.Execute(() =>
            {
                Console.WriteLine("Task 3 started.");
                Task.Delay(3500).Wait();
                Console.WriteLine("Task 3 done.");
            });

            return semaphore;

            //Task.WaitAll(task1, task2, task3);
        }
    }
}
