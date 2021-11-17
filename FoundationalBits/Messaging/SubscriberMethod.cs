using System;
using System.Reflection;
using MonadicBits;

namespace bridgefield.FoundationalBits.Messaging
{
    using static Functional;

    public sealed record SubscriberMethod(MethodInfo Method, Type ArgumentType)
    {
        public Maybe<object> Invoke(object target, object argument) => 
            Method.Invoke(target, new[] {argument}) ?? Nothing;
    }
}