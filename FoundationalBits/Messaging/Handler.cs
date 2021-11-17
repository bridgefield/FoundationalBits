using System.Threading.Tasks;

namespace bridgefield.FoundationalBits.Messaging
{
    public sealed record Handler(object Target, SubscriberMethod Method)
    {
        public Task Post(object argument) =>
            Method.Invoke(Target, argument).Match(
                j => j switch
                {
                    Task t => t,
                    _ => Task.FromResult(j)
                },
                () => Task.CompletedTask);
    }
}