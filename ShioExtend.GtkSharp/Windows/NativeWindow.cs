using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Gtk;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Extensions;

using ShioExtend.GtkSharp.Internals;

namespace ShioExtend.GtkSharp.Windows;

public partial class NativeWindow : CriticalFinalizerObject
{
    private static readonly Action<NativeWindow> PresentCoreAction = static (window) => window.PresentCore();
    private static readonly Action<NativeWindow> ShowCoreAction = static (window) => window.ShowCore();
    private static readonly Action<NativeWindow> HideCoreAction = static (window) => window.HideCore();
    private static readonly Action<NativeWindow> ShowDialogCoreAction = static (window) => window.ShowDialogCore();

    private readonly GCHandle _parentReference;

    private CancellationTokenSource? _dialogTokenSource;
    private Gdk.Pixbuf? _tempIcon;
    private Window? _window, _dialogParent;
    private string _tempTitle = string.Empty;
    private nuint _disposed;
    private uint _runtimeFlags, _closeReason = (uint)CloseReason.UserClicked, _dialogResult;

    public NativeWindow(NativeWindow? parent = null)
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        _parentReference = parent is null ? default : GCHandle.Alloc(parent, GCHandleType.Weak);
        _dialogTokenSource = null;
    }

    public void Present()
    {
        if (WindowMessageLoop.TryStart(this, PresentCoreAction, out _))
            return;

        WindowMessageLoop.ThrowIfNotInMessageLoopThread(mustBeInMessageLoopThread: true);
        WindowMessageLoop.ProcessAllInvoke();
        PresentCore();
    }

    public Task PresentAsync()
    {
        if (!WindowMessageLoop.TryStart(this, PresentCoreAction, out _))
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                PresentCore();
            }
            else
                return WindowMessageLoop.InvokeTaskAsync(PresentCoreAction, this);
        }

        return Task.CompletedTask;
    }

    public void Hide()
    {
        if (WindowMessageLoop.TryStart(this, HideCoreAction, out _))
            return;

        WindowMessageLoop.ThrowIfNotInMessageLoopThread(mustBeInMessageLoopThread: true);
        WindowMessageLoop.ProcessAllInvoke();
        HideCore();
    }

    public Task HideAsync()
    {
        if (!WindowMessageLoop.TryStart(this, HideCoreAction, out _))
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                HideCore();
            }
            else
                return WindowMessageLoop.InvokeTaskAsync(HideCoreAction, this);
        }

        return Task.CompletedTask;
    }

    public void Show()
    {
        if (WindowMessageLoop.TryStart(this, ShowCoreAction, out _))
            return;

        WindowMessageLoop.ThrowIfNotInMessageLoopThread(mustBeInMessageLoopThread: true);
        WindowMessageLoop.ProcessAllInvoke();
        ShowCore();
    }

    public Task ShowAsync()
    {
        if (!WindowMessageLoop.TryStart(this, ShowCoreAction, out _))
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                ShowCore();
            }
            else
                return WindowMessageLoop.InvokeTaskAsync(ShowCoreAction, this);
        }

        return Task.CompletedTask;
    }

    public DialogResult ShowDialog()
    {
        if (!WindowMessageLoop.TryStart(this, ShowDialogCoreAction, out _))
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread(mustBeInMessageLoopThread: true);
            WindowMessageLoop.ProcessAllInvoke();
            ShowDialogCore();
        }
        return DialogResult;
    }

    public Task<DialogResult> ShowDialogAsync()
    {
        if (!WindowMessageLoop.TryStart(this, ShowCoreAction, out _))
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                ShowDialogCore();
            }
            else
                return AsyncCore();
        }

        return Task.FromResult((DialogResult)Atomics.Read(ref _dialogResult));

        async Task<DialogResult> AsyncCore()
        {
            await WindowMessageLoop.InvokeTaskAsync(ShowDialogCoreAction, this);
            return (DialogResult)Atomics.Read(ref _dialogResult);
        }
    }

    private void PresentCore() => GetOrCreateWindow().PresentWithTime(Global.CurrentEventTime);

    private void HideCore()
    {
        Window? window = _window;
        if (window is null)
            return;

        HideCore(window);
    }

    protected virtual void HideCore(Window window) => window.Hide();

    internal Window ShowCore()
    {
        Window window = GetOrCreateWindow();
        ShowCore(window);
        return window;
    }

    protected virtual void ShowCore(Window window) => window.Show();

    internal void ShowDialogCore()
    {
        Window window = ShowCore();
        Window? parent = FindParentWindowForDialog(window);
        window.Modal = true;

        Atomics.Write(ref _dialogParent, parent);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Atomics.Write(ref _dialogTokenSource, tokenSource);
        WindowMessageLoop.StartMiniLoop(tokenSource.Token);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Close() => Close(CloseReason.Programmically);

    public void Close(CloseReason reason)
    {
        Window? window = _window;
        if (window is null)
            return;
        Atomics.Write(ref _closeReason, (uint)reason);
        window.Close();
    }

    private static Window? FindParentWindowForDialog(Window window)
    {
        Window? parent = window.TransientFor;
        if (parent is not null)
            return parent;

        parent = NativeWindowManager.FindActiveWindow()?.Window;
        window.TransientFor = parent;
        return parent;
    }

    private Window GetOrCreateWindow()
    {
        Window? result;
        if (GetRuntimeFlagsDirectly().HasFlagFast(WindowRuntimeFlags.Initialized))
        {
            result = _window!;
        }
        else
        {
            GCHandle reference = _parentReference;
            Window? parentWindow;
            if (reference != default && reference.Target is NativeWindow parent)
                parentWindow = parent.Window;
            else
                parentWindow = null;

            result = CreateWindow(parentWindow);

            if (result.Handle == IntPtr.Zero)
                InvalidOperationException.Throw("Cannot create the window!");

            if (!NativeWindowManager.TryRegisterWindow(this))
                InvalidOperationException.Throw("Cannot register the window!");
            Atomics.Write(ref _window, result);
            HookWindowEvents(result);
            OnWindowCreated(result);
        }

        return result;
    }

    protected virtual void ChangeIconCore(Window window, Gdk.Pixbuf? icon) => window.Icon = icon;

    protected virtual void ChangeTitleCore(Window window, string title) => window.Title = title;
}
