using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingleSand.TcpServer
{
    public interface IClientSession : IDisposable
    {
        event Action<IClientSession> Finish;
        IClientHandler Handler { get; }
        Task Communicate(CancellationToken cancellation);
    }
}