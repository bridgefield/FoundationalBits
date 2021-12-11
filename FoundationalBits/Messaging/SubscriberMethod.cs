using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonadicBits;

namespace bridgefield.FoundationalBits.Messaging
{
    internal sealed record SubscriberMethod(MethodInfo Method, Type ArgumentType)
    {
        public Maybe<object> Invoke(object target, object argument)
        {
            try
            {
                return Method.Invoke(target, new[] { argument }).JustNotNull();
            }
            catch (Exception exception)
            {
                throw DispatchFailed.Handle(target, exception);
            }
        }

        public static IEnumerable<SubscriberMethod> ForType(Type type) =>
            type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name == nameof(IHandle<int>.Handle)
                            && m.GetParameters().Length == 1)
                .Select(FromMethod);

        private static SubscriberMethod FromMethod(MethodInfo method) =>
            new(method, method.GetParameters().First().ParameterType);
    }
}