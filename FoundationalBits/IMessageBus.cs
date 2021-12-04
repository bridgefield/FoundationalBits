using System.Threading.Tasks;

namespace bridgefield.FoundationalBits
{
    public interface IMessageBus
    {
        void Subscribe(object subscriber);
        void Unsubscribe(object subscriber);
        Task Publish(object argument);
    }
}