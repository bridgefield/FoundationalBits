using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace bridgefield.FoundationalBits.Agents
{
    internal sealed class StatelessAgent<TCommand, TReply> : IAgent<TCommand, TReply>
    {
        private readonly ActionBlock<(TCommand command, TaskCompletionSource<TReply> task)> actionBlock;

        public StatelessAgent(Func<TCommand, Task<TReply>> processor) =>
            actionBlock = new(
                data => processor(data.command)
                    .HandleResult(
                        data.task.SetResult,
                        data.task.SetException)
            );

        public Task<TReply> Tell(TCommand command)
        {
            var completionSource = new TaskCompletionSource<TReply>(TaskCreationOptions.RunContinuationsAsynchronously);
            actionBlock.Post((command, completionSource));
            return completionSource.Task;
        }
    }
}