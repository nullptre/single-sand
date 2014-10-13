using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NLog;
using RabbitMQ.Client;
using SingleSand.Amq;
using SingleSand.Amq.AccessModel;
using SingleSand.Samples.Amq.Contracts;
using SingleSand.Samples.Messages;
using SingleSand.Utils.Serialization;

namespace SingleSand.Samples.Amq.Client
{
	class Program
	{
		private const string ServerQueueName = "testQ1-processor";

		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private static string _clientName;

		static void Main(string[] args)
		{
			if (args.Length == 0)
				throw new ArgumentNullException("args", "Client name should be supplied as the first argument");
			_clientName = args[0];

			Console.WriteLine("Starting Producer(Client) App");
			Tasks.EventLoop.Run(Run, true);
			Console.WriteLine("App exit");
		}

		private static async Task Run()
		{
			var clientQueueName = "testQ1-client-" + Process.GetCurrentProcess().Id;

			using (var container = Bootsrap(clientQueueName))
			using (var cancellation = new CancellationTokenSource())
			using (var rpcListener = container.Resolve<IQueueAccessFactory>().GetRpc(clientQueueName))
			{
				var publisher = container.Resolve<IQueueAccessFactory>().GetPublisher(ServerQueueName);

				try
				{
					ConsoleKey key;
					do
					{
						key = await Task.Run(() => Console.ReadKey().Key, cancellation.Token);

						if (key == ConsoleKey.Q) continue;

						var messagesTask = Enumerable.Range(0, 10)
							.Select(i => SendMessage(publisher, i, rpcListener, cancellation));

						await Task.WhenAll(messagesTask);
					} while (key != ConsoleKey.Q);
				}
				finally
				{
					cancellation.Cancel();
				}
			}
		}

		private static async Task SendMessage(IPublisher publisher, int index, IRpcListener rpcListener, CancellationTokenSource cancellation)
		{
			var result = await publisher
				.CallRemotely(
					new TextMessage
					{
						Text = string.Format(
							"Hello from Sender {0}, #{1}", _clientName, index)
					},
					rpcListener,
					new ReceiveArgs(m => true, TimeSpan.FromSeconds(5),
						cancellation.Token));

			Log.Info("Message received: {0}", result.Cast<TextMessage>().Single().Text);
		}

		private static IUnityContainer Bootsrap(string clientQueueName)
		{
			var c = new UnityContainer();
			Bootstrapper.SetUp(c);
			Formatter.SetUp();
			c.RegisterType<ISerializer, Formatter>(new ContainerControlledLifetimeManager());
			CreateQueue(clientQueueName);
			return c;
		}

		private static void CreateQueue(string queueName)
		{
			var factory = new ConnectionFactory { HostName = "localhost" };
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
