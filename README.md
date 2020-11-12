
  ![SimpleTracer](logo_128.png)

# SimpleTracer

Simplest way to listen and collect events for .Net/.Net Core applications.!



|Build|Test|Coverage|Nuget|
|--|--|--|--|
|[![Build Status](https://dev.azure.com/buraktungut/SimpleTracer/_apis/build/status/btungut.SimpleTracer?branchName=master)](https://dev.azure.com/buraktungut/SimpleTracer/_build/latest?definitionId=2)|[![Azure DevOps tests](https://img.shields.io/azure-devops/tests/buraktungut/simpletracer/2)](https://dev.azure.com/buraktungut/SimpleTracer/_build/latest?definitionId=2)|[![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/buraktungut/simpletracer/2)](https://dev.azure.com/buraktungut/SimpleTracer/_build/latest?definitionId=2)|[![Nuget](https://img.shields.io/nuget/v/SimpleTracer)](https://www.nuget.org/packages/SimpleTracer/)



If you have an application that you need to listen some events, you should implement the followings which is fairly painful;

 - Create a class that inherits from [EventListener](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventlistener)
 - Call `EnableEvents(...)` method for every source and try to eliminate the concurrency problem
 - To listen multiple different sources, be familiar with bitwise operations

Here we go! `OnEventWritten(...)` method will be triggered for every written events and you should concern;

 - **Concurrency** because events might be written from different threads
 - **Buffering** because events count per specific time might be more than you can handle this.
 - **Separation** because you wouldn't like to handle every event in same way

SimpleTracer addresses many of the common concern to provide fault tolerant, thread safe and TPL friendly solution.


### Simplest way to collect Garbage Collection events, ever!
You can configure how often and which method will be triggered by calling `WithInterval` and `WithDelegate` methods. Also `WithMaxTake` could be handy to set event limit for each trigger.
```csharp
var builder = SubscriptionContainerBuilder
    .New()
    .WithSubscription(s => s
        .WithEvents(e => e.DotNetRuntime.GC.AllInformationals())
        .WithExecution(e => e
            .WithInterval(TimeSpan.FromSeconds(30))
            .WithDelegate(OnEventsCollected)
            .WithMaxTake(300)));

var container = builder.Build();
container.Start();
```


### You could store it wherever you want
SimpleTracer supports `awaitable` methods. Every execution occurs in a `Task` which was dedicated per Subscription for your workload!
```csharp
private static Task OnEventsCollected(IEventNotification arg)
{
    foreach (IEventEntry item in arg.Events)
        Console.WriteLine($"{item.CreatedOn}\t{item.Id} : {item.Name}");

    return Task.CompletedTask;
}
```


### Multiple EventSource in one Subscription
You don't have to create different subscriptions for every EventSource.
One subscription can listen multiple `EventSource` with various `EventLevel` and `EventKeywords`.
```csharp
 var builder = SubscriptionContainerBuilder
     .New()
     .WithSubscription(s => s
         .WithEvents(e => e.DotNetRuntime.GC.AllInformationals())
         .WithEvents(e => e.SystemNetHttp.AllWarnings())
         .WithEvents(new SourceDefinition("Custom-Event-Source", EventLevel.Error, EventKeywords.All))
         .WithExecution(e => e
             .WithInterval(TimeSpan.FromSeconds(3))
             .WithDelegate(OnEventsCollected)
             .WithMaxTake(300))
         .WithOptions());

var container = builder.Build();
container.Start();
```


### Don't want to involve too much details?
You can create a Subscription to listen **all of EventSource** objects with `EventLevel` filter.
It gives you to a massive insight about your application but also more than a thousand events to be queued.

**Don't worry!** By calling `WithQueueCapacity`, it is possible to limit to be enqueued event count.
```csharp
var builder = SubscriptionContainerBuilder
    .New()
    .WithSubscription(EventLevel.Error, c => c
         .WithExecution(e => e
            .WithInterval(TimeSpan.FromSeconds(3))
            .WithDelegate(OnEventsCollected)
            .WithMaxTake(300))
        .WithOptions(o => o
            .WithQueueCapacity(3000)));

var container = builder.Build();
container.Start();
```

