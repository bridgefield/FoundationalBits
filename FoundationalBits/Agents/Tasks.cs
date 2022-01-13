using System;
using System.Threading.Tasks;

namespace bridgefield.FoundationalBits.Agents
{
    internal static class Tasks
    {
        public static Task HandleResult<T>(
            this Task<T> task,
            Action<T> onSuccess,
            Action<Exception> onError) =>
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    onError(task.Exception);
                }
                else
                {
                    onSuccess(task.Result);
                }
            });
    }
}