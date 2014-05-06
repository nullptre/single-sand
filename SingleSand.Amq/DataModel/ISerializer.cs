namespace SingleSand.Amq.DataModel
{
    public interface ISerializer
    {
        T Deserialize<T>(byte[] body);
        byte[] Serialize<T>(T instance);
    }
}