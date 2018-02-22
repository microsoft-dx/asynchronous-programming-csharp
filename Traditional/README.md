## Traditional async operations

Back in the early days of my general and high school, the only way you could write asynchronous code in C# 2.0 and Visual Studio 2005/2008 was by either implementing the [Event-based Asynchronous Pattern](https://msdn.microsoft.com/en-us/library/ms228969(v=vs.110).aspx) (EAP) or [Asynchronous Programming Model](https://msdn.microsoft.com/ro-ro/library/ms228963(v=vs.110).aspx) (APM) (both are now obsolute as of C# 4.0 - thank God 😌) both of which were very hard to understand. write and debug. I implemented the APM in [this post](https://laurentiu.microsoft.pub.ro/2016/04/21/using-delegates-in-csharp/) where I talked about [asynchronous delegates](https://github.com/microsoft-dx/advanced-csharp/blob/master/AdvancedCSharp/Delegates/Examples/AsynchronousDelegates.cs). The APM uses the [IAsyncResult](https://msdn.microsoft.com/ro-ro/library/system.iasyncresult(v=vs.110).aspx) design pattern which is implemented as two methods named **Begin**_OperationName_ and **End**_OperationName_ that begin and end the asynchronous operation _OperationName_ respectively.

The flaws of the APM appear immediately:
*   in order to finish the thread you **must** call the **End**_OperationName_ method from within the callback function
*   no easy way to cancel the async operation because you have no control whats happening in the background once the **Begin**_OperationName_ started (you can make use of a [CancellationTokenSource](https://msdn.microsoft.com/en-us/library/system.threading.cancellationtokensource(v=vs.110).aspx) which adds extra complexity to the code)
*   callbacks are not synchronized with the caller thread meaning that you will have to add extra complexity either by using the [CompletedSynchronously](https://msdn.microsoft.com/ro-ro/library/system.iasyncresult.completedsynchronously(v=vs.110).aspx) property of the [IAsyncResult](https://msdn.microsoft.com/ro-ro/library/system.iasyncresult(v=vs.110).aspx) or implementing your own synchronization algorithm
*   multiple callback synchronization is even harder as you would have to use [WaitHandles](https://msdn.microsoft.com/en-us/library/system.threading.waithandle(v=vs.110).aspx)

The EAP makes use of [delegates](https://laurentiu.microsoft.pub.ro/2016/04/21/using-delegates-in-csharp/) and [events](https://msdn.microsoft.com/en-us/library/8627sbea.aspx) and describes one way for classes to present asynchronous behavior. The events are raised on another thread, so we need a synchronization mechanism for this (check out the differences between a Thread and a Task <del>here</del>). Let's review [the code](https://github.com/microsoft-dx/advanced-csharp/tree/master/AdvancedCSharp/AsynchronousProgramming/Traditional/EAP) while showcasing its functionality.

```csharp
var primeNumberCalculator = new Traditional.PrimeNumberCalculator();

primeNumberCalculator.CalculatePrimeCompleted += new Traditional.PrimeNumberCalculator.CalculatePrimeCompletedEventHandler(CalculatePrimeCompleted);
primeNumberCalculator.ProgressChanged += new Traditional.PrimeNumberCalculator.ProgressChangedEventHandler(ProgressChanged);
```

Besides instantiating the main entry point class, we need to create new instances of events and create delegate methods as seen below.

```csharp
static void CalculatePrimeCompleted(object sender, CalculatePrimeCompletedEventArgs e)
{
    Console.WriteLine("[EAP] The number is {0}",
        e.IsPrime ? "prime" : "not prime");
}

static void ProgressChanged(ProgressChangedEventArgs e)
{
    // Do something when the progress changes
}
```

The `ProgressChanged` event fires whenever a prime number was found that is less than the number to test and after it is added to the `primeNumberList`.

```csharp
primeNumberCalculator.CalculatePrimeAsync(1299827, Guid.NewGuid());

/*
 * Thread synchronization done the easy way
 * 
 * We need to hang the current thread to wait for the
 * prime number calculator to finish processing
 */
Thread.Sleep(5000);

primeNumberCalculator.CalculatePrimeAsync(12345678, Guid.NewGuid());

/*
 * Notice how for small numbers the wait is to high
 * and for big numbers the wait to low
 * 
 * In order to implement a better thread synchronization you will need
 * to write extra complexity which is not the purpose of this example
 */ 
Thread.Sleep(5000);
```

In order to have control over the newly created threads, we want to assign a [Guid](https://msdn.microsoft.com/en-us/library/system.guid(v=vs.110).aspx) to every thread and perhaps keep track of each thread status via the [Guid](https://msdn.microsoft.com/en-us/library/system.guid(v=vs.110).aspx). In this specific example I use [Thread.Sleep](https://msdn.microsoft.com/en-us/library/system.threading.thread.sleep(v=vs.110).aspx) method to mimic thread synchronization (never use this kind of logic in production environments). I've used the [Thread.Sleep](https://msdn.microsoft.com/en-us/library/system.threading.thread.sleep(v=vs.110).aspx) approach because writting thread synchronization logic isn't the topic of this blog post. In production enviorments, this logic can also be application-specific.own

Let's talk code now. To make use of delegates and events we obviously need private [delegate](https://github.com/microsoft-dx/advanced-csharp/blob/master/AdvancedCSharp/AsynchronousProgramming/Traditional/EAP/PrimeNumberCalculator.cs#L13) and [event](https://github.com/microsoft-dx/advanced-csharp/blob/master/AdvancedCSharp/AsynchronousProgramming/Traditional/EAP/PrimeNumberCalculator.cs#L23) properties inside our class (**first** unnecessary added complexity). If we want to write as less code as possible, we would want to pass as much information as we can to the [EventArgs](https://msdn.microsoft.com/en-us/library/system.eventargs(v=vs.110).aspx) objects which we [custom build](https://github.com/microsoft-dx/advanced-csharp/tree/master/AdvancedCSharp/AsynchronousProgramming/Traditional/EAP/EventArgs) (**second** unnecessary added complexity). We also need [notification mechanisms](https://github.com/microsoft-dx/advanced-csharp/blob/master/AdvancedCSharp/AsynchronousProgramming/Traditional/EAP/PrimeNumberCalculator.cs#L183) in order to proper fire up the events/delegates (**third** unnecessary added complexity). We also need thread resource locking using the [lock statement](https://msdn.microsoft.com/ro-ro/library/c5kehkcz.aspx) (**fourth** unnecessary added complexity). All in all, the unnecessary added complexity adds up to approximately 200 lines of code and when you compare that to the entire project which has approximately 300-350 lines you figure that you wrote a lot of unnecessary code (more than a half).

## Modern async operations

As of C# 4.0, you can write asynchronous code using the [Task-based Asynchronous Pattern](https://msdn.microsoft.com/en-us/library/hh873175(v=vs.110).aspx) (TAP) which provides a more easy to implement and to debug code and presents the asynchronous operations into a single method and combines the status of the operation and the API used for interacting with those operations into a single object (the [System.Threading.Tasks.Task](https://msdn.microsoft.com/en-us/library/system.threading.tasks.task(v=vs.110).aspx) and [System.Threading.Tasks.Task<TResult>](https://msdn.microsoft.com/en-us/library/dd321424(v=vs.110).aspx) types in the [System.Threading.Tasks](https://msdn.microsoft.com/en-us/library/system.threading.tasks(v=vs.110).aspx) namespace). As of .NET 4.5, most of the classes with support for APM also have Async methods (as a standard, every method that makes use of the TAP model is named _OperationName_**Async**) already implemented and you can wrap the APM into a TAP model using the [TaskFactory.FromAsync](https://msdn.microsoft.com/en-us/library/system.threading.tasks.taskfactory.fromasync(v=vs.110).aspx) or method. If you want to run an operation on another thread, you can use the [Task.Run](https://msdn.microsoft.com/en-us/library/system.threading.tasks.task.run(v=vs.110).aspx) method by making use of lamba methods (coming soon 😎).

Let's review the flaws of the APM and how the TAP model handles them:
*   cancellation support  by default, implemented in the [Task](https://msdn.microsoft.com/en-us/library/system.threading.tasks.task(v=vs.110).aspx) class
*   automatic thread synchronization
*   easy multiple asynchronous operations coordination by making use of the [Task.ContinueWith](https://msdn.microsoft.com/en-us/library/system.threading.tasks.task.continuewith(v=vs.110).aspx) methods and its overloads.

Let's review [the code](https://github.com/microsoft-dx/advanced-csharp/tree/master/AdvancedCSharp/AsynchronousProgramming/Traditional/TAP) (the rewriting of the [EAP example](https://github.com/microsoft-dx/advanced-csharp/tree/master/AdvancedCSharp/AsynchronousProgramming/Traditional/EAP) to TAP) while showcasing its functionality and seeing how it differs from the EAP model.

```csharp
static async Task TAPExample()
{
    //var primeNumberCalculator = new Modern.PrimeNumberCalculator(displayProgress: false);
    var primeNumberCalculator = new Modern.PrimeNumberCalculator(displayProgress: true);

    Console.WriteLine("[TAP] The first number is {0}",
        await primeNumberCalculator.CalculatePrimeAsync(1299827) ? "prime" : "not prime");

    Console.WriteLine("[TAP] The second number is {0}",
        await primeNumberCalculator.CalculatePrimeAsync(1234567) ? "prime" : "not prime");
}
```

Notice how we only need to instantiate the main entry point class without the need for extra properties. As you may have observed in my [previous post](https://laurentiu.microsoft.pub.ro/2016/08/07/asynchronous-programming-in-csharp/), the asynchronous code look rather synchronous, more readable, and the result can be stored in a variable, in the same method.

```csharp
public Task<bool> CalculatePrimeAsync(int numberToTest)
{
    var firstDivisor = 1;

    var task = Task
        .FromResult(BuildPrimeNumberList(numberToTest))
        .ContinueWith((prevTask) => IsPrime(prevTask.Result, numberToTest, out firstDivisor));

    return task;
}
```

This is all the asynchronous logic you need to write, right here in this method. As mentioned above, we wrap synchronous code into the TAP model using the [Task.FromResult](https://msdn.microsoft.com/en-us/library/hh194922(v=vs.110).aspx) method and we [Task.ContinueWith](https://msdn.microsoft.com/en-us/library/dd270696(v=vs.110).aspx) passing the `primeNumberList` to the `IsPrime` method thus initiating the algorithm.

And that is all. Seriously, that is all the asynchronous code we had to write, that very method above, the rest in algorithm specific code. If we were to talk code, the whole application has aproximately 100 lines of code and the TAP asynchronous logic has aproximately 20 lines. If we look at the EAP application, we see a huge improvement in both word count and "reinvent the wheel" code.

## In conclusion

As you may have observed, a bit of C# coding knowledge is required. For a better understanding of how powerful the C# language really is, you can check out [this repository](https://github.com/microsoft-dx/csharp-fundamentals/) full with basic C# projects. If you want to go deeply into advanced C# topics, you can check out [this repository](https://github.com/microsoft-dx/advanced-csharp). Stay tuned on this blog (and star the [microsoft-dx organization](https://github.com/microsoft-dx/)) to emerge in the beautiful world of “there’s an app for that”.

Post image source: [wikimedia.org](https://upload.wikimedia.org/wikipedia/commons/c/cf/2012_WTCC_Race_of_Japan_(Race_1)_opening_lap.jpg)

Example adapted from: [msdn.microsoft.com](https://msdn.microsoft.com/en-us/library/bz33kx67(v=vs.110).aspx)
