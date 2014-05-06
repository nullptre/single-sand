using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SingleSand.TcpServer
{
    public interface ITcpServer : IDisposable
    {
        Task ListenIncomingClients(CancellationToken cancellation);
        ICollection<IClientSession> ActiveClients { get; }
    }
}