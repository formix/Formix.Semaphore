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

The rsulting trace shows that the Semaphore ensured that its value was never exceeded by running tasks usage:
```
*** Semaphore Created. Value = 10 ***
- Task 0 created, Usage = 5
- Task 1 created, Usage = 2
- Task 2 created, Usage = 10
- Task 3 created, Usage = 2
- Task 4 created, Usage = 4
- Task 5 created, Usage = 9
- Task 6 created, Usage = 5
- Task 7 created, Usage = 3
- Task 8 created, Usage = 1
- Task 9 created, Usage = 8
[1] Task 1, usage 2, Started
[1] Task 0, usage 5, Started
TotalTasksCount: 10
RunningTasksCount: 2
RunningTasksUsage: 7/10
[1] Task 1, usage 2, Running
[1] Task 0, usage 5, Running
[1] Task 1, usage 2, Done
[1] Task 0, usage 5, Done
TotalTasksCount: 9
RunningTasksCount: 1
RunningTasksUsage: 5/10
TotalTasksCount: 8
[118] Task 2, usage 10, Started
RunningTasksUsage: 10/10
[118] Task 2, usage 10, Running
[118] Task 2, usage 10, Done
RunningTasksCount: 0
[214] Task 3, usage 2, Started
RunningTasksUsage: 0/10
[214] Task 3, usage 2, Running
TotalTasksCount: 7
RunningTasksCount: 1
RunningTasksUsage: 2/10
[227] Task 4, usage 4, Started
RunningTasksCount: 2
RunningTasksUsage: 6/10
[214] Task 3, usage 2, Done
[227] Task 4, usage 4, Running
[227] Task 4, usage 4, Done
TotalTasksCount: 6
RunningTasksCount: 1
RunningTasksUsage: 4/10
[368] Task 5, usage 9, Started
TotalTasksCount: 5
RunningTasksUsage: 9/10
[368] Task 5, usage 9, Running
[368] Task 5, usage 9, Done
RunningTasksCount: 0
RunningTasksUsage: 0/10
[478] Task 8, usage 1, Started
TotalTasksCount: 4
[492] Task 6, usage 5, Started
[492] Task 7, usage 3, Started
RunningTasksCount: 3
RunningTasksUsage: 9/10
[478] Task 8, usage 1, Running
[492] Task 7, usage 3, Running
[492] Task 6, usage 5, Running
[492] Task 6, usage 5, Done
[492] Task 7, usage 3, Done
[478] Task 8, usage 1, Done
TotalTasksCount: 3
RunningTasksCount: 2
RunningTasksUsage: 4/10
TotalTasksCount: 2
RunningTasksCount: 1
RunningTasksUsage: 1/10
[620] Task 9, usage 8, Started
RunningTasksUsage: 8/10
TotalTasksCount: 1
[620] Task 9, usage 8, Running
[620] Task 9, usage 8, Done
RunningTasksCount: 0
RunningTasksUsage: 0/10
TotalTasksCount: 0
```