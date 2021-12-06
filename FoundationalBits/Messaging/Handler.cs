using System.Threading.Tasks;

namespace bridgefield.FoundationalBits.Messaging
{
    public sealed record Handler(object Target, SubscriberMethod Method)
    {
        public Task Post(object argument) =>
            Task.Run(() => Method.Invoke(Target, argument)
                .Match(
                    j => j switch
                    {
                        Task t => t,
                        _ => Task.FromResult(j)
                    },
                    () => Task.CompletedTask)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        throw DispatchFailed.Handle(Target, t.Exception);
                    }
                }));
    }
}