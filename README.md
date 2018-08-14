# Formix.Semaphore
In-process .NET Standard implementation of an awaitable semaphore.

# Usage
A semaphore is an abstraction on a resource that is available in limited 
quantity. For example, you could have a subscription to an online service
that limits the number of simultaneous connection from your account to two.
To make sure you don't try to use more connection than you should, just run
your code in an asynchronous method inside the semaphore.

```c#
        var semaphore = Semaphore.Initialize("connections", 2);

        var task1 = semaphore.Execute(() =>
        {
            Console.WriteLine("Task 1 started.");
            Task.Delay(250).Wait();
            Console.WriteLine("Task 1 done.");
        });

        var task2 = semaphore.Execute(() =>
        {
            Console.WriteLine("Task 2 started.");
            Task.Delay(500).Wait();
            Console.WriteLine("Task 2 done.");
        });

        var task3 = semaphore.Execute(() =>
        {
            Console.WriteLine("Task 3 started.");
            Task.Delay(350).Wait();
            Console.WriteLine("Task 3 done.");
        });

        Task.WaitAll(task1, task2, task3);
```

The output should look something like that:

```
Task 1 started.
Task 2 started.
Task 1 done.
Task 3 started.
Task 2 done.
Task 3 done.
```
