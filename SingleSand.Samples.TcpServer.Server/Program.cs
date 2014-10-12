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
		private static IUnityContainer _container;
		private static ITcpServer _tcpServer;
		private static CancellationTokenSource _cancellation;

		static void Main(string[] args)
		{
			Console.WriteLine("Starting Tcp Server App in thread {0}", Thread.CurrentThread.ManagedThreadId);
			Tasks.Utils.Run(Run, true);
			Console.WriteLine("App exit");
		}

		private static async Task Run()
		{
			using (_container = Bootsrap())
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

		static IUnityContainer Bootsrap()
		{
			var c = new UnityContainer();
			Bootstrapper.SetUp(c);
			Formatter.SetUp();
			c.RegisterType<ISerializer, Formatter>(new ContainerControlledLifetimeManager());
			c.RegisterType<IClientHandlerFactory, SimpleClientHandler.Factory>(new ContainerControlledLifetimeManager());
			return c;
		}
	}
}
