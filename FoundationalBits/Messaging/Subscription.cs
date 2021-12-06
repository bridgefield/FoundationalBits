using System;
using System.Linq;
using MonadicBits;

namespace bridgefield.FoundationalBits.Messaging
{
    public sealed record Subscription(WeakReference Subscriber, SubscriberMethod[] Methods)
    {
        public bool CanHandle(Type argumentType) =>
            MethodFor(argumentType).Match(_ => true, () => false);

        public Maybe<Handler> Handler(Type argumentType) =>
            Subscriber.JustWhen(s => s.IsAlive)
                .Bind(s => s.Target.JustNotNull())
                .Bind(t => MethodFor(argumentType).Map(m => new Handler(t, m)));

        public static Subscription FromSubscriber(object subscriber) =>
            new(new WeakReference(subscriber),
                SubscriberMethod.FromType(subscriber.GetType()).ToArray());

        private Maybe<SubscriberMethod> MethodFor(Type argumentType) =>
            Methods.Where(h => h.ArgumentType.IsAssignableFrom(argumentType)).FirstOrNothing();
    }
}