using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
        public void DocumentationDemoCode()
        {
            var limitedConnSemaphore = Semaphore.Initialize("ServerConnections", 5);
            var tasks = new List<Task>();
            var rnd = new Random();

            for (int i = 0; i < 25; i++)
            {
                var taskNumber = i;
                tasks.Add(limitedConnSemaphore.Execute(async () =>
                {
                    await SaveValue(taskNumber, rnd.Next(100) + 25);
                }));
            }

            // Adds a monitor in parallel in its own task.
            tasks.Add(Task.Run(async () =>
            {
                var lastRunningTasks = 0;
                while (limitedConnSemaphore.TotalTaskCount > 0)
                {
                    if (lastRunningTasks != limitedConnSemaphore.RunningTaskCount)
                    {
                        lastRunningTasks = limitedConnSemaphore.RunningTaskCount;
                        Console.WriteLine(
                            $"Number of task running: {lastRunningTasks}");
                    }
                    await Task.Delay(1);
                }

                // 0 tasks should be running here obviously but lets write it anyway...
                Console.WriteLine(
                    $"Number of task running: {limitedConnSemaphore.RunningTaskCount}");
            }));

            Task.WaitAll(tasks.ToArray());
        }

        private async Task SaveValue(int taskNumber, int value)
        {
            // Connects to the server, do a few things, simulates the save 
            // operation...
            await Task.Delay(_rnd.Next(40) + 10);
            Console.WriteLine($"Task {taskNumber}: Saving the value {value}.");
            await Task.Delay(_rnd.Next(40) + 10);
        }

        [TestMethod]
        public void TestRunningALotOfTasks()
        {
            // Don't forget to take a look at the output, it's mesmerizing!

            const int taskCount = 20;
            var tasks = new List<Task>(taskCount + 1);
            var taskDones = new bool[taskCount];
            var rnd = new Random();

            var value = rnd.Next(10) + 3;
            var semaphore = Semaphore.Initialize("TestRunningALotOfTasks", value);
            Console.WriteLine($"*** Semaphore Created. Value = {value} ***");

            var start = DateTime.Now.Ticks / 10000;

            for (int i = 0; i < taskCount; i++)
            {
                var usage = rnd.Next(value) + 1;
                var index = i;

                Console.WriteLine($"- Task {index} created, Usage = {usage}");

                tasks.Add(semaphore.Execute(() =>
                {
                    Console.WriteLine($"[{DateTime.Now.Ticks / 10000 - start}] Task {index}, usage {usage}, Started");
                    Task.Delay(rnd.Next(40) + 10).Wait();
                    Console.WriteLine($"[{DateTime.Now.Ticks / 10000 - start}] Task {index}, usage {usage}, Running");
                    Task.Delay(rnd.Next(40) + 10).Wait();
                    Console.WriteLine($"[{DateTime.Now.Ticks / 10000 - start}] Task {index}, usage {usage}, Done");
                    taskDones[index] = true;
                    Task.Delay(rnd.Next(40) + 10).Wait();
                },
                usage));
            }


            var monitoringTask = Task.Run(async () =>
            {
                var lastRunningTaskCount = 0;
                var lastTotalTaskCount = 0;
                var lastRunningTaskUsage = 0;
                while (semaphore.TotalTaskCount > 0)
                {
                    if (lastTotalTaskCount != semaphore.TotalTaskCount)
                    {
                        lastTotalTaskCount = semaphore.TotalTaskCount;
                        Console.WriteLine($"TotalTaskCount: {lastTotalTaskCount}");
                    }
                    if (lastRunningTaskCount != semaphore.RunningTaskCount)
                    {
                        lastRunningTaskCount = semaphore.RunningTaskCount;
                        Console.WriteLine($"RunningTaskCount: {lastRunningTaskCount}");
                    }
                    if (lastRunningTaskUsage != semaphore.RunningTaskUsage)
                    {
                        lastRunningTaskUsage = semaphore.RunningTaskUsage;
                        Console.WriteLine($"RunningTaskUsage: {lastRunningTaskUsage}");
                    }
                    
                    // Make sure that no task overrun the semaphore value.
                    Assert.IsTrue(semaphore.Value >= semaphore.RunningTaskUsage);

                    await Task.Delay(5);
                }

                Console.WriteLine($"TotalTaskCount: {semaphore.TotalTaskCount}");
                Console.WriteLine($"RunningTaskCount: {semaphore.RunningTaskCount}");
                Console.WriteLine($"RunningTaskUsage: {semaphore.RunningTaskUsage}");

            });

            tasks.Add(monitoringTask);
            Task.WaitAll(tasks.ToArray());

            foreach (var taskDone in taskDones)
            {
                Assert.IsTrue(taskDone);
            }
        }
    }
}
