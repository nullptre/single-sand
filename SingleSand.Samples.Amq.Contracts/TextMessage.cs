using ProtoBuf;
using SingleSand.Amq.AccessModel;
using SingleSand.Samples.Messages;

namespace SingleSand.Samples.Amq.Contracts
{
    [ProtoContract]
    public class TextMessage : Message, IMessage
    {
        [ProtoMember(1)]
        public string Text { get; set; }
    }
}