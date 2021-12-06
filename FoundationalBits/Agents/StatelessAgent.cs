using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace bridgefield.FoundationalBits.Agents
{
    public sealed class StatelessAgent<TCommand, TReply> : IAgent<TCommand, TReply>
    {
        private readonly ActionBlock<(TCommand command, TaskCompletionSource<TReply> task)> actions;

        public StatelessAgent(Func<TCommand, Task<TReply>> processor) =>
            actions = new ActionBlock<(TCommand command, TaskCompletionSource<TReply> task)>(
                data => processor(data.command)
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            data.task.SetException(task.Exception);
                        }
                        else
                        {
                            data.task.SetResult(task.Result);
                        }
                    })
            );

        public Task<TReply> Tell(TCommand command)
        {
            var completionSource = new TaskCompletionSource<TReply>(TaskCreationOptions.RunContinuationsAsynchronously);
            actions.Post((command, completionSource));
            return completionSource.Task;
        }
    }
}