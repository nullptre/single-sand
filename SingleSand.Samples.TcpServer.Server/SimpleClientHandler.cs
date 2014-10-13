using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using SingleSand.Samples.TcpServer.Contracts;
using SingleSand.TcpServer;
using SingleSand.Utils;
using SingleSand.Utils.Serialization;

namespace SingleSand.Samples.TcpServer.Server
{
    public class SimpleClientHandler : IClientHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static int _counter;

        private readonly TcpClient _tcpClient;
        private readonly ISerializer _serializer;

        private readonly AtomicAction _atomicSend = new AtomicAction();

        private SimpleClientHandler(TcpClient tcpClient, ISerializer serializer)
        {
            _tcpClient = tcpClient;
            _serializer = serializer;
        }

        public string ClientName { get; private set; }

        public async Task Run(CancellationToken cancellation)
        {
            Log.Debug("Incomming connection detected, waiting for auth");
            var m = await await
                    Task.WhenAny(
                        _serializer.DeserializeAsync<ApiCommandWithName>(_tcpClient.GetStream(), cancellation),
                        Task.Delay(TimeSpan.FromSeconds(10), cancellation)
							.ContinueWith(t => (ApiCommandWithName) null, cancellation));
            if (m == null)
            {
                Log.Warn("Authentication timeout for connection {0}", _tcpClient.Client.Handle.ToInt32());
                return;
            }

            ClientName = m.ClientName;
            Log.Info("New client connected: {0}", ClientName);

            while (!cancellation.IsCancellationRequested)
            {
                m = await _serializer.DeserializeAsync<ApiCommandWithName>(_tcpClient.GetStream(), cancellation);
                Log.Debug("Api Command received: {0}", m.Text);

                if (m.Text == "CalculateAndRespond")
                {
	                var calculationResult = _counter++;
	                await _serializer.SerializeAsync(
                        new ApiCommandWithName { Text = string.Format("Calculation result from {0} : {1}", ClientName, calculationResult) },
                        _tcpClient.GetStream(), cancellation);
                }

	            if (m.Text == "Quit")
                {
                    Log.Info("Finishing connection {0}", ClientName);
                    return;
                }
            }
        }

        public async Task Send(ApiCommandWithName apiCommandWithName, CancellationToken cancellation)
        {
            await _atomicSend.Run(
                async () => await _serializer.SerializeAsync(apiCommandWithName, _tcpClient.GetStream(), cancellation));
        }

        public class Factory : IClientHandlerFactory
        {
            private readonly ISerializer _serializer;

            public Factory(ISerializer serializer)
            {
                _serializer = serializer;
            }

            public async Task<IClientHandler> Get(TcpClient tcpClient)
            {
                return new SimpleClientHandler(tcpClient, _serializer);
            }
        }
    }
}