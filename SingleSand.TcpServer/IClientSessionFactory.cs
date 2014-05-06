using System.Net.Sockets;
using System.Threading.Tasks;

namespace SingleSand.TcpServer
{
    public interface IClientSessionFactory
    {
        Task<IClientSession> Get(TcpClient tcpClient, IClientHandler handler);
    }
}