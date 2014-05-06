using ProtoBuf;

namespace SingleSand.Amq.DataModel
{
    [ProtoContract]
    public class TextMessage : Message
    {
        [ProtoMember(1)]
        public string Text { get; set; }
    }
}