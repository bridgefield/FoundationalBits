using System;
using System.Linq;
using MonadicBits;

namespace bridgefield.FoundationalBits.Messaging
{
    internal abstract record Subscription(SubscriberMethod[] Methods)
    {
        public abstract Maybe<object> Target { get; }

        public bool CanHandle(Type argumentType) =>
            MethodFor(argumentType).Match(_ => true, () => false);

        public Maybe<Handler> Handler(Type argumentType) =>
            Target.Bind(t => MethodFor(argumentType).Map(m => new Handler(t, m)));

        private Maybe<SubscriberMethod> MethodFor(Type argumentType) =>
            Methods.Where(h => h.ArgumentType.IsAssignableFrom(argumentType)).FirstOrNothing();
    }

    internal sealed record WeakSubscription(
        WeakReference Subscriber,
        SubscriberMethod[] Methods) : Subscription(Methods)
    {
        public override Maybe<object> Target =>
            Subscriber
                .JustWhen(s => s.IsAlive)
                .Bind(s => s.Target.JustNotNull());

        public static WeakSubscription FromSubscriber(object subscriber) =>
            new(new WeakReference(subscriber),
                SubscriberMethod.ForType(subscriber.GetType()).ToArray());
    }

    internal sealed record StrongSubscription(
        object Subscriber,
        SubscriberMethod[] Methods) : Subscription(Methods)
    {
        public override Maybe<object> Target => Subscriber;

        public static StrongSubscription FromSubscriber(object subscriber) =>
            new(subscriber, SubscriberMethod.ForType(subscriber.GetType()).ToArray());
    }
}