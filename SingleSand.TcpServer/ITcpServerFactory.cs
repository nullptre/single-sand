using System.Net;

namespace SingleSand.TcpServer
{
    public interface ITcpServerFactory
    {
        ITcpServer Get(IPAddress ip, int port);
    }
}