using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShioExtend.GtkSharp;

partial class WindowMessageLoop
{
    private static bool TryDynamicInvokeCoreAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(new DynamicInvokeClosure(@delegate, args, null, cancellationToken));

    private static Task<object?>? TryDynamicInvokeTaskCoreAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(new DynamicInvokeClosure(@delegate, args, completionSource, cancellationToken)) ? completionSource.Task : null;
    }
}
