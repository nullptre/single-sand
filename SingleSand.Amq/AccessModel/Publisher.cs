using System.Threading.Tasks;
using SingleSand.Amq.DataModel;
using SingleSand.Amq.QueueStreaming;

namespace SingleSand.Amq.AccessModel
{
    public class Publisher : IPublisher
    {
        private readonly IQueueWriter _queueWriter;

        public Publisher(IQueueWriter queueWriter)
        {
            _queueWriter = queueWriter;
        }

        public async Task Push(Message message)
        {
            await _queueWriter.Send(message);
        }
    }
}