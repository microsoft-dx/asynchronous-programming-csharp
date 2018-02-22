## What is Asynchronous Programming?

Asynchrony refers to the occurrence of events independently. Those events can be triggered "inside" to take place concurrently with another program or "outside" such as the arrival of a signal or an interrupt without the program **blocking** to **wait** for results.

You can think of the asynchronous programming model in the following way - imagine that you are at Starbucks 😎 and that the order and payment queue is the main program. You order a Cappuccino Venti and then you enjoy your coffee while relaxing on the couch or at a chat with your friend. Now, **asynchronously**, there are other people enjoying their coffee, be it Mocha, Americano etc. and you are independent from any of them. You don't have to **block** or **wait** after another person (process or program in this case) to finish his/hers coffee to start to enjoy it yourself, you just do it **in the same time**.

## Asynchronous and Parallel Programming

Parallel programming refers to **simultaneously** calculations. It operates on the principle that every problem (usually a time consuming one) can be divided into smaller ones, which are solved at the same time, thus reducing computation time.

There's a very fine line between asynchronous and parallel programming. They may seem to operate in the same field, but they solve different problems and are based on different reasoning. If we take the Starbucks 😎 example mentioned above, we could say that every person is drinking a small different portion from the same big coffee **in the same time**. If you want to learn more about parallel programming in C#, you can check <del>this blog post</del> (coming soon 😎).

## Traditional async operations

If you are a C# geek (just like me :sunglasses:) or have had programmed with C# or in the .NET environment, you can check [this blog post](https://laurentiu.microsoft.pub.ro/2016/08/14/from-eap-to-tap-in-csharp/) in which I talk about the [Event-based Asynchronous Pattern](https://msdn.microsoft.com/en-us/library/ms228969(v=vs.110).aspx) (EAP) and how you can rewrite it using the [Task-based Asynchronous Pattern](https://msdn.microsoft.com/en-us/library/hh873175(v=vs.110).aspx) (TAP).

## Modern async operations

As of C# 4.0, you can write asynchronous code using the [Task-based Asynchronous Pattern](https://msdn.microsoft.com/en-us/library/hh873175(v=vs.110).aspx) (TAP) which provides a more easy to implement and to debug code. If you want to learn more about the improvements the TAP did, you can check [this blog post](https://laurentiu.microsoft.pub.ro/2016/08/14/from-eap-to-tap-in-csharp/) in which I compare the the [Task-based Asynchronous Pattern](https://msdn.microsoft.com/en-us/library/hh873175(v=vs.110).aspx) (TAP) and the [Asynchronous Programming Model](https://msdn.microsoft.com/ro-ro/library/ms228963(v=vs.110).aspx) (APM).

## Understanding async/await keywords

With the introduction of TAP, the need for a more readable code grew because you had to write a lot of code for a basic asynchronous operation. Thus, with the introduction of C# 5.0 came the async/await keywords witch is now the standard for writing asynchronous code.

A normal C# method can be made asynchronous by specifying the `async` keyword and the return type to `Task` (you can check more about normal methods in C# [here](https://github.com/microsoft-dx/csharp-fundamentals)). If you expect your method to return a result, then you will use the generic class `Task<T>` where `T` is the type of the object your method will return. Inside the asynchronous method there should be at least one awaited block, otherwise it will give you a warning. To wait for another asynchronous method, you will use the `await` keyword before the operation. If you want to learn more about what happens under-the-hood you can check <del>this blog post</del> (coming soon 😎).

```csharp
public async Task<string> ReadFromFileAsync(string filePath)
{
    using (var fileStream = new StreamReader(filePath))
    {
        return await fileStream.ReadToEndAsync();
    }
}

public async Task WriteToFileAsync(string filePath, string data)
{
    using (var fileStream = new StreamWriter(filePath))
    {
        await fileStream.WriteLineAsync(data);
    }
}
```

Here we have a basic input-output example of asynchronous code. We make use of the [StreamReader](https://msdn.microsoft.com/en-us/library/system.io.streamreader(v=vs.110).aspx) class to open and read from a file and the [StreamWriter](https://msdn.microsoft.com/en-us/library/system.io.streamwriter(v=vs.110).aspx) class to open and write to a file. We then encapsulate this inside a disposable `using` block of code (you can check more about the `using` keyword [here](https://github.com/microsoft-dx/csharp-fundamentals)) and read/write the data. At first glance, this particularly code seems rather synchronously, don't you think? Well, that's the power of C#'s syntactic sugar I guess 😎

```csharp
public async Task<string> GetHtmlAsync(Uri webPage)
{
    using (var client = new HttpClient())
    {
        var data = await client.GetAsync(webPage);

        return await data.Content.ReadAsStringAsync();
    }
}
```

As of .NET 4.5, most of the classes have support for TPL, meaning that most of the basic operations like reading, writing to a file and, in this example, getting the HTML source code of a page is made easy with the `async/await` keywords. The principle is the same, we create an asynchronous method using the `async` keyword, we set the return type to `Task<string>` because we want to return the source code after the request is finished (we use the `await` keyword for this), we make use of the [HttpClient](https://msdn.microsoft.com/en-us/library/system.net.http.httpclient(v=vs.118).aspx) class to asynchronously get the webpage and then we return the content as string.

## Async void vs Async Task

You should always use the `async` keyword with the `Task` return type even if you want your method to not return anything thus using the `void` keyword. The first and most important issue is that `async void` methods do not benefit from TPL's exception handling mechanisms, which means that if the code crashes inside a void method, even if you surrounded the method with a `try-catch` block, the exception will not be catched. The second issue is that `async void` methods can not be awaited because they are not awaitable (check <del>this blog post</del> for more information).

```csharp
private static async void AsyncVoid()
{
    // This exception is not caught
    throw new NotImplementedException("AsyncVoid");
}

private static async Task AsyncTask()
{
    throw new NotImplementedException("AsyncTask");
}

public static async Task TryAsyncOperations()
{
    try
    {
        AsyncVoid();
        await AsyncTask();
    }
    catch (Exception e)
    {
        // This is the AsyncTask exception
        Console.WriteLine(e.Message);
    }
}
```

As you can see after the code has executed, the **AsyncVoid** exception is never caught and the program crashes whereas the **AsyncTask** exception is successfully catched by the `try-catch` block (you can check more about the `try-catch` block [here](https://github.com/microsoft-dx/csharp-fundamentals)).

## In conclusion

As you may have observed, a bit of C# coding knowledge is required. For a better understanding of how powerful the C# language really is, you can check out [this repository](https://github.com/microsoft-dx/csharp-fundamentals/) full with basic C# projects. If you want to go deeply into advanced C# topics, you can check out [this repository](https://github.com/microsoft-dx/advanced-csharp).

So there you have it, a well documented post about what asynchronous programming is and is not. Stay tuned on this blog (and star the [microsoft-dx organization](https://github.com/microsoft-dx/)) to emerge in the beautiful world of “there’s an app for that”.
