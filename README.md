single-sand
===========

# Intro

SingleSand is an experimental project based on [idea of single-threaded processing (or event loop)](http://berb.github.io/diploma-thesis/original/055_events.html).
Main benefit is that you don't need to write andy cross-thread synchronization logic, as a result your
code looks simpler, is more readable and easier to debug. You can concentrate your efforts on writing business-related tasks.

The good example of such approach is [Node.js](http://www.toptal.com/nodejs/why-the-hell-would-i-use-node-js) that successfully
handles parallel requests from many clients asynchronously. Node.js is not the only example,
there are also [eventmachine](https://github.com/eventmachine/eventmachine), [kayak](https://github.com/kayak/kayak) and more.

SingleSand is fully based on C# async programming model and TPL therefore can act as a base for
most async applications as well as a bridge between different application layers. Theoretically
the event loop can be plugged to any application type, multithreaded or not it doesn't matter,
the only requirement is to allocate some thread for the event loop. See list of samples below.

SingleSand connects several popular development tools (like Async Message Queues, TCP transport, ASP.NET handlers, etc.)
to the event loop where all business tasks run and implements interfaces for interaction between the tasks and tools.


# Getting started

Hello world example

```csharp
// You just need to post an initial async action to the event loop.
// EventLoop.Run method utilizes current thread and runs the event loop on it.
EventLoop.Run(
    async () =>
    {
        using (var reader = File.OpenText("hello_world.txt"))
        {
            // This does not block the evnet loop
            var text = await reader.ReadToEndAsync();

            Console.Write(text);
        }
    },
    true /*this means that the event loop should stop after inital action completes*/);
```

Another example, demonstrates parallel access to shared resources

```csharp
var nonThreadSafeVariable = new List<string>();
var rnd = new Random();

EventLoop.Run(
    async () =>
    {
        Func<int, Task> childTask = async () =>
        {
            var text = string.Format("sample-{0}", i);

            //note taht this code does not require any thread syncronization statements,
            //it is safe to accees the variable directly
            if (!nonThreadSafeVariable.Contains(text))
            {
                //simulate divergence
                await Task.Delay(rnd.Next(10));

                nonThreadSafeVariable.Add(text);
            }

            //and writing to console is also safe
            Console.WriteLine("Hello, this is task #{0}", i);
        };

        //start all tasks in parallel
        var parallelTasks = Enumerable.Range(0, 1000).Select(childTask).ToArray();

        //and wait for all
        await Task.WhenAll(parallelTasks);
    },
    true);
```

Advanced examples are in sampe projects, see below.

# Supported tools

At the moment they are
* Async Message Queue based on RabbitMQ
* TCP Server based on System.Net.Sockets


# Supported application types

* ASP.NET async web applications
* Windows services
* Console applications
* Theoretically it is pluggable to any other application type


# Restrictions

As any of event-loop emplementations it is required that a single event handler is short-running.
While developing an app this has to be kept in mind, otherwise the handler may prevent other handlers to
execute. All long-runnint cumputations have to be executed outside the event loop. There are
several ways to do that: the simplest Task.Run or more advanced like delegating the task to remote
process using Async Message Queue.


# Project structure

Solution is divided into two secions

* Platform

This section contains core assemblies for application development (SDK)
  ** SingleSand.Tasks - event loop implementation
  ** SingleSand.TcpServer - asynchronous TCP handler based on System.Net.Sockets
  ** SingleSand.Amq - asychronous publisher and consumer around RabbitMQ

* Samples

Examples of various application types built on the planform
  ** SingleSand.Samples.Tasks - basic console sample.
  ** SingleSand.Samples.TcpServer - TCP client-server. Single server, many clients.
  ** SingleSand.Samples.Amq.Server - RabbitMQ publisher-consumer. Many publishers, many consumers, all connected to the same queue.
  ** SingleSand.Samples.Web - ASP.NET web application. Implemented over async MVC actions.
  ** SingleSand.Samples.WinService - Windows service basic sample.

# Similar projects

* https://github.com/jacksonh/manos
* https://github.com/blesh/ALE
* http://www.kendar.org/?p=/dotnet/nodecs

# Benchmark

TODO