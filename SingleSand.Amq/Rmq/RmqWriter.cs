using System.Threading.Tasks;
using RabbitMQ.Client;
using SingleSand.Amq.AccessModel;
using SingleSand.Amq.QueueStreaming;
using SingleSand.Utils.Serialization;

namespace SingleSand.Amq.Rmq
{
    internal class RmqWriter : IQueueWriter
    {
        private readonly IModel _channel;
        private readonly ISerializer _serializer;

        public RmqWriter(string queueName, IModel channel, ISerializer serializer)
        {
            _channel = channel;
            _serializer = serializer;
            QueueName = queueName;
        }

        protected string QueueName { get; private set; }

        public async Task Send(IMessage message)
        {
            var body = Serialize(message);

            //dont block execution flow when message is being sent
            await Task.Run(() => _channel.BasicPublish(QueueName, "", null, body));
        }

        private byte[] Serialize(IMessage message)
        {
            return _serializer.Serialize(message);
        }

        public override string ToString()
        {
            return QueueName;
        }

        public void Dispose()
        {
            //do nothing
        }
    }
}