using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace bridgefield.FoundationalBits.Messaging
{
    [Serializable]
    public class DispatchFailed : Exception
    {
        public DispatchFailed(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DispatchFailed(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public static DispatchFailed Create(string source, Exception error) =>
            new($"Dispatch to {source} failed.", error);

        public static DispatchFailed Create(Exception error) =>
            new("Dispatch failed.", error);

        public static DispatchFailed Handle(object target, Exception exception) =>
            exception switch
            {
                AggregateException aggregateException when aggregateException.InnerExceptions.Count == 1 => Handle(
                    target, aggregateException.InnerExceptions.Single()),
                TargetInvocationException targetInvocationException => Create(target.ToString(),
                    targetInvocationException.InnerException),
                _ => Create(target.ToString(), exception)
            };

        public static DispatchFailed Handle(Exception exception) =>
            exception switch
            {
                AggregateException aggregateException when aggregateException.InnerExceptions.Count == 1 =>
                    Handle(aggregateException.InnerExceptions.Single()),
                DispatchFailed dispatchFailed => new DispatchFailed(
                    dispatchFailed.Message,
                    dispatchFailed.InnerException),
                _ => Create(exception)
            };
    }
}