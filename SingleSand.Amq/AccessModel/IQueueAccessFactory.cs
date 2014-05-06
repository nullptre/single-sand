using System;
using System.Threading.Tasks;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.AccessModel
{
    public interface IQueueAccessFactory
    {
        IRpcListener GetRpc(string queueName);
        IContiniousListener GetContinious(string queueName, Func<Message, Task> handler);
        IPublisher GetPublisher(string queueName);
    }
}