using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {

        private int _semaphore = 5;

        static void Main()
        {
            var app = new Program();
            app.RunSimple();
        }








        private void RunSimple()
        {
            var tasks = new Task[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                var index = i;
                tasks[i] = Task.Run(() => 
                {
                    Thread.Sleep((index + 1) * 5);
                    DoSomeStuff(index);
                });
            }
            Task.WaitAll(tasks);

            Console.Write("\nPress any key to exit...");
            Console.ReadKey(true);
        }

        private void DoSomeStuff(int i)
        {
            var rnd = new Random(i);
            var usage = rnd.Next(3) + 1;

            Wait(ref _semaphore, usage);
            Console.WriteLine($"Task {i} doing some stuff (usage {usage}) [{_semaphore}]");

            Thread.Sleep(rnd.Next(500) + 100); // Doing stuff here... really.

            Signal(ref _semaphore, usage);
            Console.WriteLine($"Task {i} finished (usage {usage}) [{_semaphore}]");
        }

        private void Wait(ref int semaphore, int usage)
        {
            while (semaphore < usage)
            {
                Thread.Sleep(50);
            }
            semaphore -= usage;
        }

        private void Signal(ref int semaphore, int usage)
        {
            semaphore += usage;
        }










    }
}
