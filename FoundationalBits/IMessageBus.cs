using System.Threading.Tasks;

namespace bridgefield.FoundationalBits
{
    public enum SubscriptionLifecycle
    {
        /// <summary>
        /// Subscribers with this life cycle flag will be unsubscribed
        /// when garbage collected.
        /// This does not prevent explicit unsubscription.
        /// </summary>
        GarbageCollected,

        /// <summary>
        /// Subscriptions are only unsubscribed when explicitly called.
        /// </summary>
        ExplicitUnsubscribe
    }

    public interface IMessageBus
    {
        /// <summary>
        /// Create subscription with <see cref="SubscriptionLifecycle.GarbageCollected"/>
        /// life cycle.
        /// </summary>
        void Subscribe(object subscriber);

        void Subscribe(object subscriber, SubscriptionLifecycle lifecycle);

        void Unsubscribe(object subscriber);

        Task Publish(object argument);
    }
}