using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using SingleSand.Samples.TcpServer.Contracts;
using SingleSand.Utils.Serialization;

namespace SingleSand.Samples.TcpServer.Client
{
    class Program
    {
        private static string _clientName;

        static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentNullException("args", "Client name should be supplied as the first argument");
            _clientName = args[0];

            Console.WriteLine("Starting Tcp Client App for client {0}", _clientName);
            Tasks.EventLoop.Run(Run, true);
            Console.WriteLine("App exit");
        }

        private static async Task Run()
        {
            using (var container = Bootsrap())
            using (var tcp = new TcpClient("127.0.0.1", 10345))
            using (var cancellation = new CancellationTokenSource())
            {
                Console.WriteLine("Tcp Connection created");

                var tcpInterationTask = AcceptTcpResponses(tcp, container, cancellation.Token);
                var userInterationTask = AcceptUserInput(container, tcp, cancellation);

                var finishedTask = await Task.WhenAny(userInterationTask, tcpInterationTask);
                await finishedTask; //this line throws error if any

                if (finishedTask != tcpInterationTask)
                    //let tcpInterationTask finish corretly
                    await tcpInterationTask;
            }
        }

        private static async Task AcceptUserInput(IUnityContainer c, TcpClient tcp, CancellationTokenSource cancellation)
        {
            try
            {
                ConsoleKey key;
                do
                {
                    key = await Task.Run(() => Console.ReadKey().Key);

                    switch (key)
                    {
                        case ConsoleKey.A:
                            await
                                c.Resolve<ISerializer>()
                                    .SerializeAsync(new ApiCommandWithName {ClientName = _clientName}, tcp.GetStream(),
                                        cancellation.Token);
                            break;
                        case ConsoleKey.C:
                            await
                                c.Resolve<ISerializer>()
                                    .SerializeAsync(new ApiCommandWithName {Text = "CalculateAndRespond"}, tcp.GetStream(),
                                        cancellation.Token);
                            break;
                        case ConsoleKey.E:
                            await
                                c.Resolve<ISerializer>()
                                    .SerializeAsync(new ApiCommandWithName {Text = "Quit"}, tcp.GetStream(),
                                        cancellation.Token);
                            break;
                    }

                }
                while (key != ConsoleKey.Q && !cancellation.Token.IsCancellationRequested);
            }
            finally
            {
                cancellation.Cancel();
            }
        }

        private static async Task AcceptTcpResponses(TcpClient tcp, IUnityContainer c, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ApiCommandWithName command;
                try
                {
                    command = await
                              c.Resolve<ISerializer>().DeserializeAsync<ApiCommandWithName>(tcp.GetStream(), cancellationToken);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
                catch (SerializationException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
                if (command != null)
                {
                    Console.WriteLine("Tcp server responds: {0}", command.Text);
                }
            }
        }

        static IUnityContainer Bootsrap()
        {
            var c = new UnityContainer();
            Messages.Formatter.SetUp();
            c.RegisterType<ISerializer, Messages.Formatter>(new ContainerControlledLifetimeManager());
            return c;
        }
    }
}
