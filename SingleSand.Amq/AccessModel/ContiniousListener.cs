using System;
using System.Threading.Tasks;
using SingleSand.Amq.DataModel;
using SingleSand.Amq.QueueStreaming;

namespace SingleSand.Amq.AccessModel
{
    internal class ContiniousListener : IContiniousListener
    {
        private readonly IQueueReader _queueReader;
        private readonly Func<Message, Task> _handler;

        public ContiniousListener(IQueueReader reader, Func<Message, Task> handler)
        {
            this._queueReader = reader;
            _handler = handler;
            this._queueReader.NewMessage += this.OnNewMessageReceived;
        }

        private async Task OnNewMessageReceived(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");
            await _handler(message);
        }

        public void Dispose()
        {
            this._queueReader.NewMessage -= this.OnNewMessageReceived;
        }
    }
}