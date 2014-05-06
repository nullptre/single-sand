using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using SingleSand.Utils;

namespace SingleSand.TcpServer
{
    internal class TcpServer : ITcpServer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int MaxConnectionsInQueue = 100000;

        private readonly TcpListener _listener;
        private readonly IClientSessionFactory _sessionFactory;
        private readonly IClientHandlerFactory _handlerFactory;
        private readonly IDictionary<IClientSession, Task> _clients = new Dictionary<IClientSession, Task>();

        public TcpServer(IPAddress ip, int port, IClientSessionFactory sessionFactory, IClientHandlerFactory handlerFactory)
        {
            _sessionFactory = sessionFactory;
            _handlerFactory = handlerFactory;
            _listener = new TcpListener(ip, port);
        }

        public ICollection<IClientSession> ActiveClients
        {
            get { return _clients.Keys; }
        }

        public async Task ListenIncomingClients(CancellationToken cancellation)
        {
            _listener.Start(MaxConnectionsInQueue);
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var newClient = await _listener.AcceptTcpClientAsync()
                        .CancelWith(() => (TcpClient) null, cancellation);
                    if (newClient == null)
                        continue;
                    var handler = await _handlerFactory.Get(newClient);
                    var newSession = await _sessionFactory.Get(newClient, handler);
                    newSession.Finish += OnSessionFinish;
                    var clientTask = ClientCommunicate(cancellation, newSession);
                    _clients.Add(newSession, clientTask);
                }
            }
            finally
            {
                _listener.Stop();
            }
            await Task.WhenAll(_clients.Values);
        }

        private static async Task ClientCommunicate(CancellationToken cancellation, IClientSession session)
        {
            using (session)
            {
                Log.Debug("Starting client session {0}", session);
                await session.Communicate(cancellation);
                Log.Debug("Finishing client session {0}", session);
            }
        }

        private void OnSessionFinish(IClientSession session)
        {
            session.Finish -= OnSessionFinish;
            _clients.Remove(session);
        }

        public void Dispose()
        {
            foreach (var client in _clients)
            {
                client.Key.Dispose();
            }
            _clients.Clear();
            _listener.Server.Dispose();
        }
    }
}
