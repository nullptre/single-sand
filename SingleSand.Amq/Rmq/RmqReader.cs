using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SingleSand.Amq.AccessModel;
using SingleSand.Amq.QueueStreaming;
using SingleSand.Utils.Serialization;

namespace SingleSand.Amq.Rmq
{
    internal class RmqReader : IQueueReader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IModel _channel;
        private readonly ISerializer _serializer;
        private readonly EventingBasicConsumer _consumer;
        private readonly string _consumerTag;

        private readonly TaskScheduler _taskScheduler;

        public RmqReader(string queueName, IModel channel, ISerializer serializer)
        {
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            _channel = channel;
            _serializer = serializer;
            QueueName = queueName;

            _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += OnReceived;

            _consumerTag = _channel.BasicConsume(queueName, false, _consumer);
        }

        private void OnReceived(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            Func<Task> asyncChain = async () =>
                {
                    try
                    {
                        if (NewMessage != null)
                            await NewMessage(DeserializeMessage(args));
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Receive from {0} failed: {1}", QueueName, e);
                        throw;
                    }
                    finally
                    {
                        _channel.BasicAck(args.DeliveryTag, false);
                    }
                };
            //the event handler should run in captured SynchronizationContext
            Task.Factory.StartNew(asyncChain, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        private IMessage DeserializeMessage(BasicDeliverEventArgs args)
        {
            return _serializer.Deserialize<IMessage>(args.Body);
        }

        public event Func<IMessage, Task> NewMessage;

        public string QueueName { get; private set; }

        public void Dispose()
        {
            _channel.BasicCancel(_consumerTag);
            _consumer.Received -= OnReceived;
        }

        public override string ToString()
        {
            return QueueName;
        }
    }
}