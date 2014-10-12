using System.Net;

namespace SingleSand.TcpServer
{
	internal class TcpServerFactory : ITcpServerFactory
    {
        private readonly IClientSessionFactory _sessionFactory;
        private readonly IClientHandlerFactory _handlerFactory;

        public TcpServerFactory(IClientSessionFactory sessionFactory, IClientHandlerFactory handlerFactory)
        {
            _sessionFactory = sessionFactory;
            _handlerFactory = handlerFactory;
        }

        public ITcpServer Get(IPAddress ip, int port)
        {
            return new TcpServer(ip, port, _sessionFactory, _handlerFactory);
        }
    }
}