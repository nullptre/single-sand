using ProtoBuf;

namespace SingleSand.Samples.Messages
{
    [ProtoContract]
    public abstract class Message
    {
        [ProtoMember(1)]
        public long? ConversationId { get; set; }

        [ProtoMember(2)]
        public string ResponseQueueName { get; set; }
    }
}