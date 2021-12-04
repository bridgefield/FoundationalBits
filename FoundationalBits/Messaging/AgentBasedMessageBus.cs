using System;
using System.Linq;
using System.Threading.Tasks;

namespace bridgefield.FoundationalBits.Messaging
{
    public sealed class AgentBasedMessageBus : IMessageBus
    {
        private sealed record State(Subscription[] Subscriptions);

        private readonly IAgent<SubscriptionCommand, Subscription[]> agent;

        public AgentBasedMessageBus() =>
            agent = IAgent<SubscriptionCommand, Subscription[]>.Start(
                new State(Array.Empty<Subscription>()),
                (state, command) => command.Execute(state));

        public void Subscribe(object subscriber) =>
            agent.Tell(new AddSubscription(Subscription.FromSubscriber(subscriber)));

        public void Unsubscribe(object subscriber) =>
            agent.Tell(new UnsubscribeTarget(subscriber));

        public async Task Publish(object argument)
        {
            try
            {
                Task.WaitAll(
                    (await agent.Tell(new SelectSubscriptions(argument.GetType())))
                    .Select(s => s.Handler(argument.GetType())
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
            public abstract Task<(State, Subscription[])> Execute(State state);
        }

        private sealed record AddSubscription(Subscription NewSubscription) : SubscriptionCommand
        {
            public override Task<(State, Subscription[])> Execute(State state) =>
                AsTask(Unpack(state with
                {
                    Subscriptions = state.Subscriptions.Concat(new[] { NewSubscription }).ToArray()
                }));
        }

        private sealed record UnsubscribeTarget(object Target) : SubscriptionCommand
        {
            public override Task<(State, Subscription[])> Execute(State state) =>
                AsTask(Unpack(state with
                {
                    Subscriptions = state.Subscriptions.Except(
                        state.Subscriptions
                            .Where(s => s.Subscriber.Target == Target)).ToArray()
                }));
        }

        private sealed record RemoveSubscription(Subscription Subscription) : SubscriptionCommand
        {
            public override Task<(State, Subscription[])> Execute(State state) =>
                AsTask(Unpack(state with
                {
                    Subscriptions = state.Subscriptions.Except(new[] { Subscription }).ToArray()
                }));
        }

        private sealed record SelectSubscriptions(Type ArgumentType) : SubscriptionCommand
        {
            public override Task<(State, Subscription[])> Execute(State state) =>
                AsTask((state,
                    state.Subscriptions.Where(s => s.CanHandle(ArgumentType)).ToArray()));
        }

        private static (State State, Subscription[] Results) Unpack(State state) =>
            (state, state.Subscriptions);

        private static Task<(State State, Subscription[] Results)>
            AsTask((State State, Subscription[] Results) tuple) =>
            Task.FromResult(tuple);
    }
}