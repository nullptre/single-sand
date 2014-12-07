using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using SingleSand.Samples.Messages;
using SingleSand.TcpServer;
using SingleSand.Utils.Serialization;

namespace SingleSand.Samples.TcpServer.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Tcp Server App in thread {0}", Thread.CurrentThread.ManagedThreadId);
            Tasks.EventLoop.Run(Run, true);
            Console.WriteLine("App exit");
        }

        private static async Task Run()
        {
            using (var container = Bootsrap())
            using (var tcpServer = container.Resolve<TcpServerFactory>()
                .Get(IPAddress.Parse("127.0.0.1"), 10345, container.Resolve<IClientHandlerFactory>()))
            using (var cancellation = new CancellationTokenSource())
            {
                var tcpServerTask = tcpServer.ListenIncomingClients(cancellation.Token);
                var userInterationTask = AcceptUserInput(cancellation);

                var finishedTask = await Task.WhenAny(userInterationTask, tcpServerTask);
                await finishedTask; //this line throws error if any

                if (finishedTask != tcpServerTask)
                    //let tcpServerTask finish corretly
                    await tcpServerTask;
            }
        }

        private static async Task AcceptUserInput(CancellationTokenSource cancellation)
        {
            try
            {
                ConsoleKey key;
                do
                {
                    key = await Task.Run(() => Console.ReadKey().Key);
                } while (key != ConsoleKey.Q);
            }
            finally
            {
                cancellation.Cancel();
            }
        }

        private static IUnityContainer Bootsrap()
        {
            var c = new UnityContainer();
            c.RegisterInstance(TcpServerFactory.Default);
            Formatter.SetUp();
            c.RegisterType<ISerializer, Formatter>(new ContainerControlledLifetimeManager());
            c.RegisterType<IClientHandlerFactory, SimpleClientHandler.Factory>(new ContainerControlledLifetimeManager());
            return c;
        }
    }
}
