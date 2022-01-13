using System.Threading.Tasks;

namespace bridgefield.FoundationalBits
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IAgent<TCommand, TReply>
    {
        Task<TReply> Tell(TCommand command);
    }
}