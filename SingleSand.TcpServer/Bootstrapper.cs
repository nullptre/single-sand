using Microsoft.Practices.Unity;

namespace SingleSand.TcpServer
{
    public static class Bootstrapper
    {
         public static void SetUp(IUnityContainer c)
         {
             c.RegisterType<ITcpServerFactory, TcpServerFactory>(new ContainerControlledLifetimeManager());
             c.RegisterType<IClientSessionFactory, ClientSessionFactory>(new ContainerControlledLifetimeManager());
         }
    }
}