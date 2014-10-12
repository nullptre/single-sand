namespace SingleSand.Utils.Serialization
{
    public interface ISerializer
    {
        T Deserialize<T>(byte[] body);
        byte[] Serialize<T>(T instance);
    }
}