using System;
using System.Threading.Tasks;
using bridgefield.FoundationalBits.Agents;

namespace bridgefield.FoundationalBits
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IAgent<TCommand, TReply>
    {
        Task<TReply> Tell(TCommand command);

        static IAgent<TCommand, TReply> Start<TState>(
            TState initialState,
            Func<TState, TCommand, Task<(TState newState, TReply reply)>> update)
            => new StatefulAgent<TState, TCommand, TReply>(initialState, update);

        static IAgent<TCommand, TReply> Start(Func<TCommand, Task<TReply>> update)
            => new StatelessAgent<TCommand, TReply>(update);
    }
}