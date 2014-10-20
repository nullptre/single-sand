using System.Threading.Tasks;
using SingleSand.Amq.QueueStreaming;

namespace SingleSand.Amq.AccessModel
{
	internal class Publisher : IPublisher
    {
        private readonly IQueueWriter _queueWriter;

        public Publisher(IQueueWriter queueWriter)
        {
            _queueWriter = queueWriter;
        }

        public async Task Push(IMessage message)
        {
            await _queueWriter.Send(message);
        }
    }
}