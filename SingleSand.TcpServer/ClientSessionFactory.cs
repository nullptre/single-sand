using System.Net.Sockets;
using System.Threading.Tasks;

namespace SingleSand.TcpServer
{
    internal class ClientSessionFactory : IClientSessionFactory
    {
        public Task<IClientSession> Get(TcpClient tcpClient, IClientHandler handler)
        {
            return Task.FromResult<IClientSession>(new ClientSession(tcpClient, handler));
        }
    }
}