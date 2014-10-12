using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using SingleSand.Amq;
using SingleSand.Amq.AccessModel;
using SingleSand.Samples.Amq.Contracts;
using SingleSand.Samples.Messages;
using SingleSand.Samples.TcpServer.Contracts;
using SingleSand.Tasks;
using SingleSand.Utils.Serialization;

namespace SingleSand.Samples.SenderApplication
{
    class Program
    {
        private static string _clientName;

        static void Main(string[] args)
        {
            _clientName = args[0];

            Console.WriteLine("Starting Sender App");
            Tasks.Utils.Run(Run, true);
            Console.WriteLine("App exit");
        }

        private static async Task Run()
        {
            using (var c = Bootsrap())
            using (var tcp = new TcpClient("127.0.0.1", 10345))
            using (var cancellation = new CancellationTokenSource())
            {
                Console.WriteLine("Tcp Connection created");

                var tcpResponseListeningTask = AcceptTcpResponses(tcp, c, cancellation.Token);

                var finishedTask = await Task.WhenAny(
                    AcceptUserInput(c, tcp, cancellation), tcpResponseListeningTask);
                await finishedTask; //useful in case of error
                if (finishedTask != tcpResponseListeningTask)
                    await tcpResponseListeningTask;

                await tcpResponseListeningTask;
            }
        }

        private static async Task AcceptUserInput(IUnityContainer c, TcpClient tcp, CancellationTokenSource cancellation)
        {
            var rpcListener = c.Resolve<IQueueAccessFactory>().GetRpc("testQ1-sender");//TODO use unique queue
            var publisher = c.Resolve<IQueueAccessFactory>().GetPublisher("testQ1-processor");

            try
            {
                ConsoleKey key;
                do
                {
                    key = await Task.Run(() => Console.ReadKey().Key);

                    if (key == ConsoleKey.Q) continue;

                    if (key == ConsoleKey.A)
                    {
                        await
                            c.Resolve<ISerializer>()
                             .SerializeAsync(new ApiCommandWithName {ClientName = _clientName}, tcp.GetStream(),
                                             cancellation.Token);
                        continue;
                    }

                    if (key == ConsoleKey.C)
                    {
                        await
                            c.Resolve<ISerializer>()
                             .SerializeAsync(new ApiCommandWithName {Text = "CalculateAndRespond"}, tcp.GetStream(),
                                             cancellation.Token);
                        continue;
                    }

                    if (key == ConsoleKey.E)
                    {
                        await
                            c.Resolve<ISerializer>()
                             .SerializeAsync(new ApiCommandWithName {Text = "Quit"}, tcp.GetStream(),
                                             cancellation.Token);
                        continue;
                    }

                    var messagesTask = Enumerable.Range(0, 10)
                        .Select(i => publisher
                            .CallRemotely(
                                new TextMessage
                                    {
                                        Text = string.Format(
                                        "Hello from Sender {0}, #{1}", _clientName, i)
                                    },
                                rpcListener,
                                new ReceiveArgs(m => true, TimeSpan.FromSeconds(5),
                                                cancellation.Token))
                            .ContinueWith(t => 
                                Console.WriteLine("Message received: {0}",
                                                  t.Result.Cast<TextMessage>().Single().Text)));

                    await Task.WhenAll(messagesTask);
                } while (key != ConsoleKey.Q);
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
            Bootstrapper.SetUp(c);
            return c;
        }
    }
}
