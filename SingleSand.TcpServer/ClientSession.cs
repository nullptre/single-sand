using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace SingleSand.TcpServer
{
    internal class ClientSession : IClientSession
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int ClientSocketPollIntervalSeconds = 10;
        private const int SocketErrorCodeWsaewouldblock = 10035;

        private readonly TcpClient _tcpClient;
        private readonly IClientHandler _handler;

        public ClientSession(TcpClient tcpClient, IClientHandler handler)
        {
            _tcpClient = tcpClient;
            _handler = handler;
        }

        public IClientHandler Handler
        {
            get { return _handler; }
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }

        public event Action<IClientSession> Finish;

        public async Task Communicate(CancellationToken cancellation)
        {
            try
            {
                using (var connectionCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
                {
                    var connectionStateTask = PollSocket(connectionCancellation);
                    Func<Task> handlingTask = async () =>
                        {
                            try
                            {
                                await _handler.Run(connectionCancellation.Token);
                            }
                            catch (IOException e)
                            {
                                Log.Warn("Client {0} communication ended due to IO error: {1}", this, e.Message);
                            }
                            catch (TaskCanceledException)
                            {
                                Log.Debug("Client {0} communication cancelled", this);
                            }
                            catch (Exception e)
                            {
                                Log.Error("Client {0} communication ended unexpectedly: {1}", this, e);
                                throw;
                            }
                            finally
                            {
                                connectionCancellation.Cancel();
                            }
                        };
                    await Task.WhenAll(handlingTask(), connectionStateTask);
                }
            }
            finally
            {
                if (Finish != null)
                    Finish(this);
            }
        }

        private async Task PollSocket(CancellationTokenSource connectionCancellation)
        {
            while (
                await Task.Delay(
                    TimeSpan.FromSeconds(ClientSocketPollIntervalSeconds), connectionCancellation.Token)
                .ContinueWith(t => !connectionCancellation.Token.IsCancellationRequested))
            {
                if (!_tcpClient.Connected)
                {
                    Log.Debug("Connection switched to disconnected state");
                    connectionCancellation.Cancel();
                    return;
                }

                //see http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.connected.aspx
                var client = _tcpClient.Client;
                bool blockingState = client.Blocking;
                try
                {
                    var tmp = new byte[1];
                    client.Blocking = false;
                    await Task.Run(() => client.Send(tmp, 0, 0));
                }
                catch (SocketException e)
                {
                    if (!e.NativeErrorCode.Equals(SocketErrorCodeWsaewouldblock))
                    {
                        Log.DebugException("Client disconnected", e);
                        connectionCancellation.Cancel();
                        return;
                    }
                }
                finally
                {
                    client.Blocking = blockingState;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}", _tcpClient.Client.Handle.ToInt32());
        }
    }
}