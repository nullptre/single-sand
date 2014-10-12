using Microsoft.Practices.Unity;
using SingleSand.Amq.AccessModel;
using SingleSand.Amq.QueueStreaming;
using SingleSand.Amq.Rmq;

namespace SingleSand.Amq
{
    public static class Bootstrapper
    {
         public static void SetUp(IUnityContainer c)
         {
             c.RegisterType<IQueueReaderFactory, RmqReaderFactory>(new ContainerControlledLifetimeManager());
             c.RegisterType<IQueueWriterFactory, RmqWriterFactory>(new ContainerControlledLifetimeManager());
             c.RegisterType<IQueueAccessFactory, QueueAccessFactory>(new ContainerControlledLifetimeManager());
         }
    }
}