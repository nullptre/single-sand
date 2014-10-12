using System.Collections.Generic;
using RabbitMQ.Client;
using SingleSand.Amq.QueueStreaming;
using SingleSand.Utils.Serialization;

namespace SingleSand.Amq.Rmq
{
    internal class RmqWriterFactory : IQueueWriterFactory
    {
        private readonly ISerializer _serializer;
        private readonly IDictionary<string, RmqWriter> _writers = new Dictionary<string, RmqWriter>();
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RmqWriterFactory(ISerializer serializer)
        {
            _serializer = serializer;
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public IQueueWriter Get(string queueName)
        {
            RmqWriter result;
            if (!_writers.TryGetValue(queueName, out result))
            {
                result = new RmqWriter(queueName, _channel, _serializer);
                _writers.Add(queueName, result);
            }
            return result;
        }

        public void Dispose()
        {
            foreach (var writer in _writers)
            {
                writer.Value.Dispose();
            }
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}