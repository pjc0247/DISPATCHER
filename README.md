DISPATCHER
====
Simple implementation of the `SynchronizationContext`.

WhatWhatWhat
----
As default, codes after the `await` keyword, may not be executed on the same thread. If you want to make your codes be executed in the same thread, you should implement the 'SynchronizationContext'.<br>
Here is an simple implementation of the 'SynchronizationContext', and the below usages shows up how to use my 'CustomSynchronizationContext'. 

Usage
----
```c#
static async void Foo()
{
    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

    await Task.Delay(1000);
    
    /* The below code will be executed in main thread. */
    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
}
static void Main(string[] args)
{
    var syncContext = CustomSynchronizationContext.SetCurrent();
    
    Foo();
    
    while(true)
    {
        /* This method must be called periodically */
        syncContext.DispatchAll();
        
        /* TODO : .... */
    }
}
```

Usage with PostSharp
----
```c#
public class Test
{
    /* This method will be executed in main thread. */
    [Dispatched]
    public void Foo()
    {
        Console.Write("Dispatched ");
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
    }

    /* This method will be executed in background thread. */
    [Background]
    public void Bar()
    {
        Console.Write("Background ");
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

        Foo();
    }
}
```
```c#
class Program
{
    static Test test;

    static void Main(string[] args)
    {
        var syncContext = CustomSynchronizationContext.SetCurrent();
        
        Console.Write("Main ");
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        
        test = new Test();
        test.Bar();

        while (true)
        {
            syncContext.DispatchAll();
        }
    }
}
```

__Ouptut__
```
Main 1
Background 3
Dispatched 1
```

Usages of each API
----
I've written a comment each method on my 'CustomSynchronizationContext' class. So plz seeeeeeeeeeeee DAT.
<br>
https://github.com/pjc0247/DISPATCHER/blob/master/src/CustomSynchronizationContext.cs
