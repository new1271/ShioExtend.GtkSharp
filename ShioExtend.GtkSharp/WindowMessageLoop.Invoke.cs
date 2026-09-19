using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp;

partial class WindowMessageLoop
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryPostInvokeClosure(IInvokeClosure closure)
    {
        InvokeIdleHandler.AddInvoke(closure);
        return TryPostInvokeMessage();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryPostInvokeMessage()
    {
        if (MathHelper.ToBooleanUnsafe(Atomics.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt)))
            return Atomics.Read(ref _isStarted) != 0;

        if (Atomics.Read(ref _isStarted) != 0)
        {
            GLib.Idle.Add(priority: GLib.Priority.DefaultIdle, InvokeIdleHandler.HandlerDelegate);
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ProcessAllInvoke() => InvokeIdleHandler.ProcessAllInvoke();
}
