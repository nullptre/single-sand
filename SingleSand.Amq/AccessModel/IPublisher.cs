using System.Threading.Tasks;

namespace SingleSand.Amq.AccessModel
{
    public interface IPublisher
    {
        Task Push(IMessage message);
    }
}