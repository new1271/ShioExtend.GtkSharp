using System;
using System.Runtime.CompilerServices;
using System.Threading;

using Gtk;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioExtend.GtkSharp.Internals;

namespace ShioExtend.GtkSharp.Windows;

partial class NativeWindow
{
    private void HookWindowEvents(Window window)
    {
        window.Hidden += HandleHidden;
        window.Shown += HandleShown;
        window.DeleteEvent += HandleDeleteEvent;
    }

    private void UnhookWindowEvents(Window window)
    {
        window.Hidden -= HandleHidden;
        window.Shown -= HandleShown;
        window.DeleteEvent -= HandleDeleteEvent;
    }

    private void HandleHidden(object? sender, EventArgs args)
    {
        WindowRuntimeFlags runtimeFlags = GetRuntimeFlagsDirectly();
        if (!runtimeFlags.HasFlagFast(WindowRuntimeFlags.Loaded))
        {
            RuntimeFlags = runtimeFlags | WindowRuntimeFlags.Loaded;
            OnLoaded();
        }
        OnHidden();
    }

    private void HandleShown(object? sender, EventArgs args)
    {
        WindowRuntimeFlags runtimeFlags = GetRuntimeFlagsDirectly();
        if (!runtimeFlags.HasFlagFast(WindowRuntimeFlags.Loaded))
        {
            RuntimeFlags = runtimeFlags | WindowRuntimeFlags.Loaded;
            OnLoaded();
        }
        OnShown();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void HandleDeleteEvent(object? sender, DeleteEventArgs args)
    {
        if (HandleClose())
        {
            args.RetVal = true;
            return;
        }
        HandleDestroyed();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleClose()
    {
        ClosingEventArgs args = new ClosingEventArgs((CloseReason)Atomics.Exchange(ref _closeReason, (uint)CloseReason.UserClicked), cancelled: false);
        OnClosing(ref args);
        if (args.Cancelled)
            return true;
        OnClosed();
        Atomics.Exchange(ref _dialogParent, null)?.Present();
        return false;
    }

    private void HandleDestroyed()
    {
        if (Atomics.Exchange(ref _disposed, UnsafeHelper.GetMaxValue<nuint>()) == 0)
            DisposeCore(disposing: true);

        HandleDestroyedCore();
    }

    private void HandleDestroyedCore()
    {
        WindowRuntimeFlags runtimeFlags = GetRuntimeFlagsDirectly();
        if (runtimeFlags != WindowRuntimeFlags.Destroyed)
        {
            RuntimeFlags = WindowRuntimeFlags.Destroyed;

            Window? window = _window;
            if (window is not null)
            {
                Atomics.Write(ref _window, null);
                if (!NativeWindowManager.TryUnregisterWindow(this))
                    DebugHelper.Throw();
                UnhookWindowEvents(window);
                CancellationTokenSource? dialogTokenSource = Atomics.Exchange(ref _dialogTokenSource, null);
                if (dialogTokenSource is not null)
                {
                    try
                    {
                        dialogTokenSource.Cancel(throwOnFirstException: false);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        dialogTokenSource.Dispose();
                    }
                }
            }
            OnDestroyed();
        }
    }
}
