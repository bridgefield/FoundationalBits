using System.Threading.Tasks;

namespace bridgefield.FoundationalBits.Messaging
{
    public interface IMessageBus
    {
        void Subscribe(object subscriber);
        void Unsubscribe(object subscriber);
        Task Publish(object argument);
    }
}