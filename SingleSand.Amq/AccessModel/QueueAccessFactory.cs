using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SingleSand.Amq.QueueStreaming;

namespace SingleSand.Amq.AccessModel
{
    public class QueueAccessFactory : IQueueAccessFactory
    {
        private readonly IQueueReaderFactory _readerFactory;
        private readonly IDictionary<string, RpcListener> _rpcListeners = new Dictionary<string, RpcListener>();
        private readonly IDictionary<string, ContiniousListener> _continiousListeners = new Dictionary<string, ContiniousListener>();
        private readonly IQueueWriterFactory _writerFactory;
        private readonly IDictionary<string, Publisher> _publishers = new Dictionary<string, Publisher>();

        public QueueAccessFactory(IQueueReaderFactory readerFactory, IQueueWriterFactory writerFactory)
        {
            _readerFactory = readerFactory;
            _writerFactory = writerFactory;
        }

        public IRpcListener GetRpc(string queueName)
        {
            RpcListener result;
            if (!_rpcListeners.TryGetValue(queueName, out result))
            {
                result = new RpcListener(_readerFactory.Get(queueName));
                _rpcListeners.Add(queueName, result);
            }
            return result;
        }

        public IContiniousListener GetContinious(string queueName, Func<IMessage, Task> handler)
        {
            ContiniousListener result;
            if (!_continiousListeners.TryGetValue(queueName, out result))
            {
                result = new ContiniousListener(_readerFactory.Get(queueName), handler);
                _continiousListeners.Add(queueName, result);
            }
            else
            {
                throw new NotSupportedException("Multiple handlers for the same ContiniousListener are not supported");
            }
            return result;
        }

        public IPublisher GetPublisher(string queueName)
        {
            Publisher result;
            if (!_publishers.TryGetValue(queueName, out result))
            {
                result = new Publisher(_writerFactory.Get(queueName));
                _publishers.Add(queueName, result);
            }
            return result;
        }

    }
}