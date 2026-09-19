using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;

using ShioExtend.GtkSharp.Windows;

namespace ShioExtend.GtkSharp.Internals;

internal sealed class NativeWindowManager
{
    private static readonly HashSet<NativeWindow> _windowSet = new();

    private static nuint _barrier;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnterBarrier()
    {
        ref nuint barrier = ref _barrier;
        while (Atomics.Exchange(ref barrier, 1) != 0)
        {
            SpinWait wait = new SpinWait();
            while (Atomics.Read(ref barrier) != 0)
                wait.SpinOnce();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExitBarrier() => Atomics.Exchange(ref _barrier, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeWindow? FindActiveWindow()
    {
        HashSet<NativeWindow> windowSet = _windowSet;
        EnterBarrier();
        try
        {
            return windowSet.FirstOrDefault(static window => window.Window?.IsActive ?? false);
        }
        finally
        {
            ExitBarrier();
        }
    }

    public static bool TryRegisterWindow(NativeWindow owner)
    {
        HashSet<NativeWindow> windowSet = _windowSet;
        EnterBarrier();
        try
        {
            return windowSet.Add(owner);
        }
        finally
        {
            ExitBarrier();
            GC.KeepAlive(owner);
        }
    }

    public static bool TryUnregisterWindow(NativeWindow owner)
    {
        HashSet<NativeWindow> windowSet = _windowSet;
        EnterBarrier();
        try
        {
            return windowSet.Remove(owner);
        }
        finally
        {
            ExitBarrier();
            GC.KeepAlive(owner);
        }
    }
}
