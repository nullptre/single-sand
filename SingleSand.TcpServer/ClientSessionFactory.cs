using System.Net.Sockets;
using System.Threading.Tasks;

namespace SingleSand.TcpServer
{
	internal class ClientSessionFactory : IClientSessionFactory
    {
        public async Task<IClientSession> Get(TcpClient tcpClient, IClientHandler handler)
        {
            return new ClientSession(tcpClient, handler);
        }
    }
}