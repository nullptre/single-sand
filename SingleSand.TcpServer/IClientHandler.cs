using System.Threading;
using System.Threading.Tasks;

namespace SingleSand.TcpServer
{
    public interface IClientHandler
    {
        Task Run(CancellationToken cancellation);
    }
}