using ProtoBuf;

namespace SingleSand.Samples.ModelsTest1
{
    [ProtoContract]
    public class ApiCommandWithName
    {
        [ProtoMember(1)]
        public string ClientName { get; set; }

        [ProtoMember(2)]
        public string Text { get; set; }
    }
}
