using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SingleSand.Utils.Serialization
{
    public class DefaultSerializer : ISerializer
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public T Deserialize<T>(byte[] body)
        {
            using (var stream = new MemoryStream(body))
            {
                return (T) _formatter.Deserialize(stream);
            }
        }

        public byte[] Serialize<T>(T instance)
        {
            using (var stream = new MemoryStream())
            {
                _formatter.Serialize(stream, instance);
                return stream.ToArray();
            }
        }
    }
}