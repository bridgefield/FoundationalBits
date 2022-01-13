using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bridgefield.FoundationalBits;
using bridgefield.FoundationalBits.Messaging;
using FluentAssertions;
using NUnit.Framework;

namespace FoundationalBits.Spec.Messaging
{
    public static class messages_are_dispatched
    {
        [Test]
        public static async Task to_an_interested_subscriber()
        {
            var cut = MessageBus.Create();
            var monitor = new HandleMonitor<object>();
            cut.Subscribe(monitor);
            await cut.Publish(new object());
            monitor.HasReceived(1).Should().BeTrue();
        }
    }

    public static class a_subscriber_error
    {
        [TestCaseSource(nameof(ErrorSubscribers))]
        public static void is_propagated_to_sender(object subscriber)
        {
            var cut = MessageBus.Create();
            cut.Subscribe(subscriber);

            Assert.CatchAsync<DispatchFailed>(() => cut.Publish(new object()))
                .Should().Match<DispatchFailed>(e => e.InnerException is TestError);
        }

        [TestCaseSource(nameof(ErrorSubscribers))]
        public static void does_not_prevent_dispatch_to_other_subscribers(
            object subscriber)
        {
            var cut = MessageBus.Create();
            cut.Subscribe(subscriber);
            var monitor = new HandleMonitor<object>();
            cut.Subscribe(monitor);
            Assert.CatchAsync<DispatchFailed>(() => cut.Publish(new object()));
            monitor.HasReceived(1)
                .Should().BeTrue();
        }

        private static IEnumerable<object> ErrorSubscribers()
        {
            yield return new ThrowingHandler<object>();
            yield return new AsyncThrowingHandler<object>();
        }
    }

    public static class multiple_subscriber_errors
    {
        [Test]
        public static void are_collected_and_propagated_to_sender()
        {
            var errorSubscribers = new List<object>
            {
                new ThrowingHandler<object>(),
                new AsyncThrowingHandler<object>()
            };
            var cut = MessageBus.Create();
            errorSubscribers.ForEach(s => cut.Subscribe(s));
            Assert.CatchAsync<DispatchFailed>(() => cut.Publish(new object()))
                .Should().Match<DispatchFailed>(e => HasMultipleDispatchErrors(e));
        }

        private static bool HasMultipleDispatchErrors(DispatchFailed error) =>
            error.InnerException is AggregateException aggregateException
            && aggregateException.InnerExceptions.OfType<DispatchFailed>()
                .Count() == 2;
    }

    public static class messages_are_not_dispatched
    {
        [Test]
        public static async Task to_garbage_collected_subscribers()
        {
            var cut = MessageBus.Create();
            var monitor = new HandleMonitor<object>();
            await cut.SubscribeWeak(new TestHandler<object>(monitor)).WaitForCollection();
            await cut.Publish(new object());
            monitor.HasReceived(0).Should().BeTrue();
        }

        [Test]
        public static async Task to_unsubscribed_subscribers()
        {
            var cut = MessageBus.Create();
            var monitor = new HandleMonitor<object>();
            cut.SubscribeUnsubscribe(new TestHandler<object>(monitor));
            await cut.Publish(new object());
            monitor.HasReceived(0).Should().BeTrue();
        }

        [Test]
        public static async Task to_uninterested_subscribers()
        {
            var cut = MessageBus.Create();
            var monitor = new HandleMonitor<int>();
            var subscriber = new TestHandler<int>(monitor);
            cut.Subscribe(subscriber);
            await cut.Publish("Hello");
            monitor.HasReceived(0).Should().BeTrue();
        }
    }

    public sealed class HandleMonitor<T> : IHandle<T>
    {
        private readonly List<T> receivedMessages = new();

        public void Handle(T message) => receivedMessages.Add(message);

        public bool HasReceived(int messageCount) =>
            receivedMessages.Count == messageCount;
    }

    public sealed record TestHandler<T>(HandleMonitor<T> Monitor) : IHandle<T>
    {
        public void Handle(T message) => Monitor.Handle(message);
    }

    public sealed class TestError : Exception
    {
    }

    public sealed record ThrowingHandler<T> : IHandle<T>
    {
        public void Handle(T message) => throw new TestError();
    }

    public sealed record AsyncThrowingHandler<T> : IHandleAsync<T>
    {
        public async Task Handle(T message)
        {
            await Task.Yield();
            throw new TestError();
        }
    }

    public static class Extensions
    {
        public static WeakReference SubscribeWeak<T>(this IMessageBus messageBus, IHandle<T> handler)
        {
            messageBus.Subscribe(handler);
            return new WeakReference(handler);
        }

        public static void SubscribeUnsubscribe<T>(this IMessageBus messageBus, IHandle<T> handler)
        {
            messageBus.Subscribe(handler, SubscriptionLifecycle.ExplicitUnsubscribe);
            messageBus.Unsubscribe(handler);
        }

        public static async Task WaitForCollection(this WeakReference weakReference)
        {
            while (weakReference.IsAlive)
            {
                GC.Collect();
                await Task.Yield();
            }
        }
    }
}