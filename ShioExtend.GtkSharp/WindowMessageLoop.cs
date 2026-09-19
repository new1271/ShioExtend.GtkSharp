using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

using Gtk;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

using ShioExtend.GtkSharp.Windows;

using GMainContext = GLib.MainContext;

namespace ShioExtend.GtkSharp;

public static partial class WindowMessageLoop
{
    private static readonly Action<int> _stopAction = static exitCode =>
    {
        CoreWindow.DisposeAndClearAllWindows();
        _exitCode = exitCode;
        _isStarted = 0;
    };
    private static readonly Action<NativeWindow> WindowShowAction = static window => window.ShowCore();

    private static GMainContext? _context;
    private static NativeWindow? _mainWindow;
    private static nuint _isStarted;
    private static uint _invokeBarrier, _threadIdForMessageLoop;
    private static int _exitCode;
    private static bool _isFirstTimeStart;

    public static event MessageLoopExceptionEventHandler? ExceptionCaught;

    public static bool HasMessageLoop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _isStarted) != 0;
    }

    public static bool IsMessageLoopThread
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
            return messageLoopThreadId != 0 && NativeMethods.GetCurrentThreadId() == messageLoopThreadId;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNotInMessageLoopThread(bool mustBeInMessageLoopThread = false)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
        {
            if (mustBeInMessageLoopThread)
                goto Throw;
            else
                goto Notify;
        }

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
            return;

    Throw:
        InvalidOperationException.Throw("The operation needs running in message loop thread!");
        return;

    Notify:
        DebugHelper.WriteLine("The message loop thread is not exist, the operation won't be thread-safe!");
    }

    public static void Initialize()
    {
        uint threadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (threadId != 0)
            InvalidOperationException.Throw();
        Application.Init();
        SynchronizationContext.SetSynchronizationContext(null);
        Atomics.Write(ref _threadIdForMessageLoop, NativeMethods.GetCurrentThreadId());
        _context = GMainContext.Default;
    }

    public static void ChangeMainWindow(NativeWindow? mainWindow)
        => ChangeMainWindow(mainWindow, WindowShowAction);

    public static void ChangeMainWindow(NativeWindow? mainWindow, Action<NativeWindow>? changeAction)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            ThrowWhenMessageLoopThreadNotExists();
        ChangeMainWindowCore(mainWindow, changeAction, IsMessageLoopThread);
    }

    private static void ChangeMainWindowCore(NativeWindow? mainWindow, Action<NativeWindow>? changeAction, bool isMessageLoopThread)
    {
        if (mainWindow is not null)
        {
            mainWindow.Destroyed += OnWindowDestroyed;
            if (changeAction is not null)
                Invoke(changeAction, mainWindow);
        }
        NativeWindow? oldWindow = Atomics.Exchange(ref _mainWindow, mainWindow);
        if (oldWindow is not null && !ReferenceEquals(oldWindow, mainWindow))
            oldWindow.Destroyed -= OnWindowDestroyed;

        static void OnWindowDestroyed(object? sender, EventArgs e) => Stop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Start() => Start(mainWindow: null, startAction: null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Start(NativeWindow? mainWindow) => Start(mainWindow, WindowShowAction);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Start(NativeWindow? mainWindow, Action<NativeWindow>? startAction)
    {
        if (!IsMessageLoopThread)
            InvalidOperationException.Throw("The operation needs running in message loop thread!");
        if (Atomics.Exchange(ref _isStarted, 1) != 0)
            InvalidOperationException.Throw("Message loop is already exists!");
        return StartCore(mainWindow, startAction);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryStart(out int result) => TryStart(null, null, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryStart(NativeWindow? mainWindow, out int result) => TryStart(mainWindow, WindowShowAction, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryStart(NativeWindow? mainWindow, Action<NativeWindow>? startAction, out int result)
    {
        if (!IsMessageLoopThread || Atomics.Exchange(ref _isStarted, 1) != 0)
        {
            result = 0;
            return false;
        }
        result = StartCore(mainWindow, startAction);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int StartCore(NativeWindow? mainWindow, Action<NativeWindow>? startAction)
    {
        if (_isFirstTimeStart)
        {
            _isFirstTimeStart = false;
        }
        else
        {
            ProcessAllInvoke();
        }

        ChangeMainWindowCore(mainWindow, startAction, isMessageLoopThread: true);
        try
        {
            return DoMessageLoop();
        }
        finally
        {
            Atomics.Write(ref _isStarted, 0);
            ChangeMainWindowCore(null, null, isMessageLoopThread: false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Stop(int exitCode = 0) => InvokeAsync(_stopAction, exitCode);

    internal static MessageLoopExceptionEventHandler? GetExceptionEventHandler() => ExceptionCaught;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int DoMessageLoop()
    {
        GMainContext? context = _context;
        if (context is null)
            return 0;
        while (Atomics.Read(ref _isStarted) != 0)
            context.RunIteration(may_block: true);
        while (context.HasPendingEvents)
            context.RunIteration(may_block: true);
        return _exitCode;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void StartMiniLoop(CancellationToken cancellationToken)
    {
        GMainContext? context;
        if (cancellationToken.IsCancellationRequested || (context = _context) is null)
            return;

        InvokeIdleHandler.ProcessAllInvoke();

        using CancellationTokenRegistration registration = cancellationToken.Register(static (state) =>
        {
            if (state is not GMainContext context)
                return;
            context.Wakeup();
        }, context, useSynchronizationContext: false);

        while (!cancellationToken.IsCancellationRequested && Atomics.Read(ref _isStarted) != 0)
            context.RunIteration(may_block: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowWhenMessageLoopThreadNotExists(bool condition)
    {
        if (!condition)
            ThrowWhenMessageLoopThreadNotExists();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ThrowWhenMessageLoopThreadNotExists<T>(T? condition) where T : class
        => condition ?? ThrowWhenMessageLoopThreadNotExists<T>();

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static T ThrowWhenMessageLoopThreadNotExists<T>()
        => throw new InvalidOperationException("The message loop thread is not exists");

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowWhenMessageLoopThreadNotExists()
        => throw new InvalidOperationException("The message loop thread is not exists");
}
