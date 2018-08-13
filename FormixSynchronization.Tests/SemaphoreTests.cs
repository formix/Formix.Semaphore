using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formix.Synchronization.Tests
{
    [TestClass]
    public class SemaphoreTests
    {
        private static Random _rnd = new Random();

        [TestMethod]
        public void TestInstanceReutilization()
        {
            var semaphore1 = Semaphore.Initialize("test", 5);
            var semaphore2 = Semaphore.Initialize("test", 5);
            Assert.AreEqual(semaphore1, semaphore2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestZeroQuantityInitialization()
        {
            Semaphore.Initialize("noname", 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestNegativeQuantityInitialization()
        {
            Semaphore.Initialize("noname", -5);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestReuseWithWrongQuantityInitialization()
        {
            var semaphore1 = Semaphore.Initialize("test", 5);
            var semaphore2 = Semaphore.Initialize("test", 6);
        }

        [TestMethod]
        public void TestMutexCriticalSection()
        {
            var itemList = new List<int>(2);
            var mutex = Semaphore.Initialize();

            var task1 = mutex.Execute(() =>
            {
                Task.Delay(50);
                itemList.Add(1);
            });

            var task2 = mutex.Execute(() =>
            {
                itemList.Add(2);
                Task.Delay(25);
            });

            Task.WaitAll(task1, task2);

            Assert.AreEqual(1, itemList[0]);
            Assert.AreEqual(2, itemList[1]);
        }


        [TestMethod]
        public void TestRunningALotOfTasks()
        {
            const int taskCount = 10;
            var tasks = new List<Task>(taskCount + 1);
            var taskFinished = new bool[taskCount];
            
            // Seeding random to something that looks like a good set of values to me
            var rnd = new Random(2); 

            // Initialize the semaphore with a random value.
            var value = rnd.Next(10) + 3;
            var semaphore = Semaphore.Initialize("TestRunningALotOfTasks", value);
            semaphore.Delay = 3;
            Console.WriteLine($"*** Semaphore Created. Value = {value} ***");
            var start = DateTime.Now.Ticks / 10000;

            // Create dummy tasks and starts them
            for (int i = 0; i < taskCount; i++)
            {
                // Randomize the semaphore usage for the task that will be started.
                var usage = rnd.Next(value) + 1;

                var index = i; // Store 'i' value in a local variable for later use in  lambda expression
                tasks.Add(semaphore.Execute(() =>
                {
                    // This is the fake task code...
                    var elapsed = DateTime.Now.Ticks / 10000 - start;
                    Console.WriteLine($"[{elapsed}] Task {index}, usage {usage}, Started");
                    Task.Delay(rnd.Next(40) + 10).Wait();
                    Console.WriteLine($"[{elapsed}] Task {index}, usage {usage}, Running");
                    Task.Delay(rnd.Next(40) + 10).Wait();
                    Console.WriteLine($"[{elapsed}] Task {index}, usage {usage}, Done");
                    taskFinished[index] = true;
                    Task.Delay(rnd.Next(40) + 10).Wait();
                },
                usage));

                Console.WriteLine($"- Task {index} created, Usage = {usage}");
            }

            // Creates a task to monitor all the other tasks
            var monitoringTask = Task.Run(async () =>
            {
                var semaphoreStatus = new Dictionary<string, int>()
                {
                    { "TotalTasksCount", 0},
                    { "RunningTasksCount", 0},
                    { "RunningTasksUsage", 0},
                };

                while (semaphore.Tasks.Count() > 0)
                {
                    // Make sure that no task overrun the semaphore value.
                    var totalUsage = semaphore.Tasks
                        .Where(t => t.IsRuning)
                        .Sum(t => t.Usage);

                    Assert.IsTrue(semaphore.Value >= totalUsage);
                    PrintSemaphoreStatus(semaphoreStatus, semaphore);
                    await Task.Delay(5);
                }

                PrintSemaphoreStatus(semaphoreStatus, semaphore);

                lock (semaphore.Tasks)
                {
                    var samaphoreTasksCount = semaphore.Tasks.Count();
                    var semaphoreRunningTasksCount = semaphore.Tasks.Where(t => t.IsRuning).Count();
                    var semaphoreRunningTasksUsage = semaphore.Tasks.Where(t => t.IsRuning).Sum(t => t.Usage);

                    Assert.AreEqual(0, samaphoreTasksCount);
                    Assert.AreEqual(0, semaphoreRunningTasksCount);
                    Assert.AreEqual(0, semaphoreRunningTasksUsage);
                }
            });

            // Adds the monitoring task to the batch and await them all
            tasks.Add(monitoringTask);
            Task.WaitAll(tasks.ToArray());

            foreach (var taskDone in taskFinished)
            {

                Assert.IsTrue(taskDone);
            }
        }

        private void PrintSemaphoreStatus(
            Dictionary<string, int> semaphoreStatuses, Semaphore semaphore)
        {
            lock (semaphore.Tasks)
            {
                var samaphoreTasksCount = semaphore.Tasks.Count();
                var semaphoreRunningTasksCount = semaphore.Tasks.Where(t => t.IsRuning).Count();
                var semaphoreRunningTasksUsage = semaphore.Tasks.Where(t => t.IsRuning).Sum(t => t.Usage);

                if (semaphoreStatuses["TotalTasksCount"] != samaphoreTasksCount)
                {
                    Console.WriteLine($"TotalTasksCount: {samaphoreTasksCount}");
                    semaphoreStatuses["TotalTasksCount"] = samaphoreTasksCount;
                }

                if (semaphoreStatuses["RunningTasksCount"] != semaphoreRunningTasksCount)
                {
                    Console.WriteLine($"RunningTasksCount: {semaphoreRunningTasksCount}");
                    semaphoreStatuses["RunningTasksCount"] = semaphoreRunningTasksCount;
                }

                if (semaphoreStatuses["RunningTasksUsage"] != semaphoreRunningTasksUsage)
                {
                    Console.WriteLine($"RunningTasksUsage: {semaphoreRunningTasksUsage}/{semaphore.Value}");
                    semaphoreStatuses["RunningTasksUsage"] = semaphoreRunningTasksUsage;
                }
            }
        }


    }
}
