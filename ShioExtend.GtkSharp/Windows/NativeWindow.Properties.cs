using System;
using System.Runtime.CompilerServices;
using System.Threading;

using GLib;

using Gtk;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioExtend.GtkSharp.Controls;

namespace ShioExtend.GtkSharp.Windows;

partial class NativeWindow
{
    private WindowRuntimeFlags RuntimeFlags
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (WindowRuntimeFlags)Atomics.Read(ref _runtimeFlags);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Atomics.Write(ref _runtimeFlags, (uint)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WindowRuntimeFlags GetRuntimeFlagsDirectly() => (WindowRuntimeFlags)_runtimeFlags;

    public DialogResult DialogResult
    {
        get => (DialogResult)Atomics.Read(ref _dialogResult);
        set => Atomics.Exchange(ref _dialogResult, (uint)value);
    }

    public bool IsDisposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _disposed) != 0;
    }

    public Window? Window
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _window);
    }

    public Gdk.Pixbuf? Icon
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Window? window = _window;
            if (window is not null)
                return window.Icon;

            return Atomics.Read(ref _tempIcon);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Window? window = _window;
            if (window is not null)
            {
                window.Icon = value;
                return;
            }

            Atomics.Write(ref _tempIcon, value);
        }
    }

    public string Title
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Window? window = _window;
            if (window is not null)
                return window.Title;

            return Atomics.Read(ref _tempTitle);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Window? window = _window;
            if (window is not null)
            {
                window.Title = value;
                return;
            }

            Atomics.Write(ref _tempTitle, value);
        }
    }
}
