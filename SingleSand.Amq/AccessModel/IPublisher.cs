using System.Threading.Tasks;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.AccessModel
{
    public interface IPublisher
    {
        Task Push(Message message);
    }
}