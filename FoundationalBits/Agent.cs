using System;
using System.Threading.Tasks;
using bridgefield.FoundationalBits.Agents;

namespace bridgefield.FoundationalBits
{
    public static class Agent
    {
        public static IAgent<TCommand, TReply> Start<TState, TCommand, TReply>(
            TState initialState,
            Func<TState, TCommand, Task<(TState newState, TReply reply)>> update)
            => new StatefulAgent<TState, TCommand, TReply>(initialState, update);

        public static IAgent<TCommand, TReply> Start<TCommand, TReply>(
            Func<TCommand, Task<TReply>> update)
            => new StatelessAgent<TCommand, TReply>(update);
    }
}