using System;
using System.Linq;
using System.Reflection;
using MonadicBits;

namespace bridgefield.FoundationalBits.Messaging
{
    public sealed record Subscription(WeakReference Subscriber, SubscriberMethod[] Methods)
    {
        public bool CanHandle(Type argumentType) =>
            Method(argumentType).Match(m => true, () => false);

        public Maybe<Handler> Handler(Type argumentType) =>
            Subscriber.JustWhen(s => s.IsAlive)
                .Bind(s => s.Target.JustNotNull())
                .Bind(t => Method(argumentType).Map(m => new Handler(t, m)));

        public static Subscription FromSubscriber(object subscriber)
        {
            var type = subscriber.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name == nameof(IHandle<int>.Handle) && m.GetParameters().Length == 1)
                .Select(m => new SubscriberMethod(m, m.GetParameters().First().ParameterType));
            return new Subscription(new WeakReference(subscriber), methods.ToArray());
        }

        private Maybe<SubscriberMethod> Method(Type argumentType) =>
            Methods.Where(h => h.ArgumentType.IsAssignableFrom(argumentType)).FirstOrNothing();
    }
}