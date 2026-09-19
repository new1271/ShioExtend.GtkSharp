using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShioExtend.GtkSharp;

partial class WindowMessageLoop
{
    private static bool TryInvokeCoreAsync(Action action, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(new ActionInvokeClosure(action, null, cancellationToken));

    private static bool TryInvokeCoreAsync<TArg>(Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(new ActionInvokeClosure<TArg>(action, arg, null, cancellationToken));

    private static bool TryInvokeCoreAsync<TArg1, TArg2>(
        Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(new ActionInvokeClosure<TArg1, TArg2>(action, arg1, arg2, null, cancellationToken));

    private static bool TryInvokeCoreAsync<TArg1, TArg2, TArg3>(
        Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(new ActionInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, null, cancellationToken));

    private static Task<bool>? TryInvokeTaskCoreAsync(Action action, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(new ActionInvokeClosure(action, completionSource, cancellationToken)) ? completionSource.Task : null;
    }

    private static Task<bool>? TryInvokeTaskCoreAsync<TArg>(Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(new ActionInvokeClosure<TArg>(action, arg, completionSource, cancellationToken)) ? completionSource.Task : null;
    }

    private static Task<bool>? TryInvokeTaskCoreAsync<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(new ActionInvokeClosure<TArg1, TArg2>(action, arg1, arg2, completionSource, cancellationToken)) ? completionSource.Task : null;
    }

    private static Task<bool>? TryInvokeTaskCoreAsync<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(new ActionInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, completionSource, cancellationToken)) ? completionSource.Task : null;
    }
}
