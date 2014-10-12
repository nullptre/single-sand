using System;
using System.Threading.Tasks;

namespace SingleSand.Amq.AccessModel
{
    public interface IQueueAccessFactory
    {
        IRpcListener GetRpc(string queueName);
        IContiniousListener GetContinious(string queueName, Func<IMessage, Task> handler);
        IPublisher GetPublisher(string queueName);
    }
}