# Formix.Synchronization
.NET Standard implementation of an awaitable semaphore.

# Usage
I am putting a test case here for quick access. I'll improve that section later.
```c#
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
```