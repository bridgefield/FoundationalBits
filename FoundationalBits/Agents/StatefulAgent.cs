using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace bridgefield.FoundationalBits.Agents
{
    public sealed class StatefulAgent<TState, TCommand, TReply> : IAgent<TCommand, TReply>
    {
        private readonly ActionBlock<(TCommand command, TaskCompletionSource<TReply> task)> actions;

        public StatefulAgent(TState initialState,
            Func<TState, TCommand, Task<(TState newState, TReply reply)>> processor)
        {
            var state = initialState;

            actions = new ActionBlock<(TCommand command, TaskCompletionSource<TReply> task)>(
                data => processor(state, data.command)
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            data.task.SetException(task.Exception);
                        }
                        else
                        {
                            state = task.Result.newState;
                            data.task.SetResult(task.Result.reply);
                        }
                    }));
        }

        public Task<TReply> Tell(TCommand command)
        {
            var completionSource = new TaskCompletionSource<TReply>(TaskCreationOptions.RunContinuationsAsynchronously);
            actions.Post((command, completionSource));
            return completionSource.Task;
        }
    }
}