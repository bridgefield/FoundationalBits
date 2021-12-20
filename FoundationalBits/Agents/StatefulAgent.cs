using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace bridgefield.FoundationalBits.Agents
{
    internal sealed class StatefulAgent<TState, TCommand, TReply> : IAgent<TCommand, TReply>
    {
        private readonly ActionBlock<(TCommand command, TaskCompletionSource<TReply> task)> actionBlock;

        public StatefulAgent(TState initialState,
            Func<TState, TCommand, Task<(TState newState, TReply reply)>> processor)
        {
            var state = initialState;

            actionBlock = new(
                data => processor(state, data.command)
                    .HandleResult(
                        r =>
                        {
                            state = r.newState;
                            data.task.SetResult(r.reply);
                        },
                        data.task.SetException));
        }

        public Task<TReply> Tell(TCommand command)
        {
            var completionSource = new TaskCompletionSource<TReply>(TaskCreationOptions.RunContinuationsAsynchronously);
            actionBlock.Post((command, completionSource));
            return completionSource.Task;
        }
    }
}