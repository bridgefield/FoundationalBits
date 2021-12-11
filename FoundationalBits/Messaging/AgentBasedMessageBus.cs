using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bridgefield.FoundationalBits.Messaging
{
    public sealed class AgentBasedMessageBus : IMessageBus
    {
        private sealed record State(ImmutableList<Subscription> Subscriptions);

        private readonly IAgent<SubscriptionCommand, IEnumerable<Subscription>> agent;

        public AgentBasedMessageBus() =>
            agent = IAgent<SubscriptionCommand, IEnumerable<Subscription>>.Start(
                new State(ImmutableList<Subscription>.Create()),
                (state, command) => command.Execute(state));

        public void Subscribe(object subscriber) =>
            Subscribe(subscriber, SubscriptionLifecycle.GarbageCollected);

        public void Subscribe(object subscriber, SubscriptionLifecycle lifecycle) =>
            agent.Tell(new AddSubscription(
                lifecycle == SubscriptionLifecycle.GarbageCollected
                    ? WeakSubscription.FromSubscriber(subscriber)
                    : StrongSubscription.FromSubscriber(subscriber)));

        public void Unsubscribe(object subscriber) =>
            agent.Tell(new UnsubscribeTarget(subscriber));

        public async Task Publish(object argument)
        {
            try
            {
                Task.WaitAll(
                    (await agent.Tell(new SelectSubscriptions(argument.GetType())))
                    .Select(s => s
                        .Handler(argument.GetType())
                        .Match(
                            h => h.Post(argument),
                            () => agent.Tell(new RemoveSubscription(s))))
                    .ToArray()
                );
            }
            catch (Exception exception)
            {
                throw DispatchFailed.Handle(exception);
            }
        }

        private abstract record SubscriptionCommand
        {
            public abstract Task<(State, IEnumerable<Subscription>)> Execute(State state);
        }

        private sealed record AddSubscription(Subscription NewSubscription) : SubscriptionCommand
        {
            public override Task<(State, IEnumerable<Subscription>)> Execute(State state) =>
                AsTask(Unpack(state with
                {
                    Subscriptions = state.Subscriptions.Add(NewSubscription)
                }));
        }

        private sealed record UnsubscribeTarget(object Target) : SubscriptionCommand
        {
            public override Task<(State, IEnumerable<Subscription>)> Execute(State state) =>
                AsTask(Unpack(state with
                {
                    Subscriptions = state.Subscriptions
                        .Where(s => s.Target.Match(t => t == Target, () => false))
                        .Aggregate(state.Subscriptions, (l, s) => l.Remove(s))
                }));
        }

        private sealed record RemoveSubscription(Subscription Subscription) : SubscriptionCommand
        {
            public override Task<(State, IEnumerable<Subscription>)> Execute(State state) =>
                AsTask(Unpack(state with
                {
                    Subscriptions = state.Subscriptions.Remove(Subscription)
                }));
        }

        private sealed record SelectSubscriptions(Type ArgumentType) : SubscriptionCommand
        {
            public override Task<(State, IEnumerable<Subscription>)> Execute(State state) =>
                AsTask((state,
                    state.Subscriptions.Where(s => s.CanHandle(ArgumentType))));
        }

        private static (State State, IEnumerable<Subscription>) Unpack(State state) =>
            (state, state.Subscriptions);

        private static Task<(State State, IEnumerable<Subscription> Results)>
            AsTask((State State, IEnumerable<Subscription> Results) tuple) =>
            Task.FromResult(tuple);
    }
}