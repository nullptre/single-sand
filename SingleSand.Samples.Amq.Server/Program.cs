using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NLog;
using RabbitMQ.Client;
using SingleSand.Amq.AccessModel;
using SingleSand.Samples.Amq.Contracts;
using SingleSand.Samples.Messages;
using SingleSand.Utils.Serialization;

namespace SingleSand.Samples.Amq.Server
{
    class Program
    {
        private const string InputQueueName = "testQ1-processor";

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static IUnityContainer _container;
        private static CancellationTokenSource _cancellation;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Consumer(Server) App in thread {0}", Thread.CurrentThread.ManagedThreadId);
            Tasks.EventLoop.Run(Run, true);
            Console.WriteLine("App exit");
        }

        private static async Task Run()
        {
            using (_container = Bootsrap())
            using (_container.Resolve<QueueAccessFactory>().GetContinious(InputQueueName, OnMessage))
            using (_cancellation = new CancellationTokenSource())
            {
                await AcceptUserInput();
            }
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

        private static async Task OnMessage(IMessage m)
        {
            var message = (TextMessage)m;
            Log.Info("Message received: {0}, thread {1}", message.Text, Thread.CurrentThread.ManagedThreadId);
            var publisher = _container.Resolve<QueueAccessFactory>().GetPublisher(m.ResponseQueueName);
            await Task.Delay(TimeSpan.FromSeconds(3), _cancellation.Token);
            await
                publisher.Push(new TextMessage
                {
                    Text = string.Format("Hello, I just processed the message '{0}'", message.Text),
                    ConversationId = m.ConversationId
                });
            Log.Info("Response message sent");
        }

        static IUnityContainer Bootsrap()
        {
            var c = new UnityContainer();
            c.RegisterType<QueueAccessFactory>(new ContainerControlledLifetimeManager(), new InjectionConstructor(typeof(ISerializer)));
            c.RegisterType<ISerializer, Formatter>(new ContainerControlledLifetimeManager());

            Formatter.SetUp();
            // below is a workaround for formatter initialization problem.
            // we should let the formatter know about expected messages.
            // to do so we need to load the assembly before any message comes.
            typeof (TextMessage).GetConstructors();

            CreateQueue(InputQueueName);
            return c;
        }

        private static void CreateQueue(string queueName)
        {
            var factory = new ConnectionFactory {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.ExchangeDeclare(queueName, "fanout", false, false, null);
                channel.QueueBind(queueName, queueName, string.Empty);
            }
        }
    }
}
