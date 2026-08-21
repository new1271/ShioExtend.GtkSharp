using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioExtend.GtkSharp;

partial class WindowMessageLoop
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PostInvokeClosure(IInvokeClosure closure)
    {
        InvokeIdleHandler.AddInvoke(closure);
        PostInvokeMessage();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PostInvokeMessage()
    {
        if (MathHelper.ToBooleanUnsafe(Atomics.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt)))
            return;
        GLib.Idle.Add(priority: GLib.Priority.DefaultIdle, InvokeIdleHandler.HandlerDelegate);
        Atomics.Write(ref _invokeBarrier, Booleans.FalseInt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ProcessAllInvoke() => InvokeIdleHandler.ProcessAllInvoke();
}
