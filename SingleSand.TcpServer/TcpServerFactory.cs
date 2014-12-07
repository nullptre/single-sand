using System.Net;

namespace SingleSand.TcpServer
{
    public class TcpServerFactory
    {
        private static TcpServerFactory _default;

        private readonly IClientSessionFactory _sessionFactory;

        public static TcpServerFactory Default
        {
            get
            {
                if (_default == null)
                    _default = new TcpServerFactory(new ClientSessionFactory());
                return _default;
            }
        }

        public TcpServerFactory(IClientSessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public ITcpServer Get(IPAddress ip, int port, IClientHandlerFactory handlerFactory)
        {
            return new TcpServer(ip, port, _sessionFactory, handlerFactory);
        }
    }
}