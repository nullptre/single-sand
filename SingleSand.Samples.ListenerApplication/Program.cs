using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using SingleSand.Amq.AccessModel;
using SingleSand.Amq.DataModel;
using SingleSand.Samples.ModelsTest1;
using SingleSand.TcpServer;
using Bootstrapper = SingleSand.Amq.Bootstrapper;

namespace SingleSand.Samples.ListenerApplication
{
    class Program
    {
        private static IUnityContainer _container;
        private static ITcpServer _tcpServer;
        private static CancellationTokenSource _cancellation;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Listener App in thread {0}", Thread.CurrentThread.ManagedThreadId);
            Amq.Runtime.Utils.Run(Run);
            Console.WriteLine("App exit");
        }

        private static async Task Run()
        {
            using (_container = Bootsrap())
            using (_container.Resolve<IQueueAccessFactory>().GetContinious("testQ1-processor", OnMessage))
            using (_tcpServer = _container.Resolve<ITcpServerFactory>().Get(IPAddress.Parse("127.0.0.1"), 10345))
            using (_cancellation = new CancellationTokenSource())
            {
                var tcpServerTask = _tcpServer.ListenIncomingClients(_cancellation.Token);

                var finishedTask = await Task.WhenAny(AcceptUserInput(), tcpServerTask);
                await finishedTask; //useful in case of error
                if (finishedTask != tcpServerTask)
                    await tcpServerTask;
            }
            _tcpServer = null;
        }

        private static async Task AcceptUserInput()
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
                _cancellation.Cancel();
            }
        }

        private static async Task OnMessage(Message m)
        {
            var message = (TextMessage) m;
            Console.WriteLine("Message received: {0}, thread {1}", message.Text, Thread.CurrentThread.ManagedThreadId);
            var publisher = _container.Resolve<IQueueAccessFactory>().GetPublisher(m.ResponseQueueName);
            await Task.Delay(TimeSpan.FromSeconds(3), _cancellation.Token);
            await
                publisher.Push(new TextMessage
                    {
                        Text = string.Format("Hello, I just processed the message '{0}'", message.Text),
                        ConversationId = m.ConversationId
                    });
            Console.WriteLine("Response message sent");

            if (_tcpServer == null)
                return;

            var client = _tcpServer.ActiveClients
                .Select(s => s.Handler).Cast<SimpleClientHandler>()
                .FirstOrDefault(h => !string.IsNullOrEmpty(h.ClientName) && message.Text.Contains(h.ClientName));
            if (client == null)
                return;
            await client.Send(
                new ApiCommandWithName {ClientName = client.ClientName, Text = string.Format("New message arrived to my queue, it seems that it is for you: {0}", message.Text)},
                _cancellation.Token);
        }

        static IUnityContainer Bootsrap()
        {
            var c = new UnityContainer();
            Bootstrapper.SetUp(c);
            TcpServer.Bootstrapper.SetUp(c);
            c.RegisterType<IClientHandlerFactory, SimpleClientHandler.Factory>(new ContainerControlledLifetimeManager());
            return c;
        }
    }
}
