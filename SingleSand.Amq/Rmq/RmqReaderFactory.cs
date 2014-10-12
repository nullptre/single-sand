using System.Collections.Generic;
using RabbitMQ.Client;
using SingleSand.Amq.QueueStreaming;
using SingleSand.Utils.Serialization;

namespace SingleSand.Amq.Rmq
{
    internal class RmqReaderFactory : IQueueReaderFactory
    {
        private readonly ISerializer _serializer;
        private readonly IDictionary<string, RmqReader> _readers = new Dictionary<string, RmqReader>();
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RmqReaderFactory(ISerializer serializer)
        {
            _serializer = serializer;
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public IQueueReader Get(string queueName)
        {
            RmqReader result;
            if (!_readers.TryGetValue(queueName, out result))
            {
                result = new RmqReader(queueName, _channel, _serializer);
                _readers.Add(queueName, result);
            }
            return result;
        }

        public void Dispose()
        {
            foreach (var reader in _readers)
            {
                reader.Value.Dispose();
            }
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}