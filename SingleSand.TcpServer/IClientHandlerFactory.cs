using System.Net.Sockets;
using System.Threading.Tasks;

namespace SingleSand.TcpServer
{
    public interface IClientHandlerFactory
    {
        Task<IClientHandler> Get(TcpClient tcpClient);
    }
}