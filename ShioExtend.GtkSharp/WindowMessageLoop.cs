using System;
using System.Runtime.CompilerServices;
using System.Threading;

using Gtk;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

using ShioExtend.GtkSharp.Windows;

using GMainContext = GLib.MainContext;

namespace ShioExtend.GtkSharp;

public static partial class WindowMessageLoop
{
    private static readonly Action<int> _stopAction = static exitCode =>
    {
        _exitCode = exitCode;
        _isStarted = 0;
    };
    private static readonly Action<CoreWindow> _windowShowAction = static window => window.ShowInternal();

    private static GMainContext? _context;
    private static CoreWindow? _mainWindow;
    private static nuint _isStarted;
    private static uint _invokeBarrier, _threadIdForMessageLoop;
    private static int _exitCode;

    public static event MessageLoopExceptionEventHandler? ExceptionCaught;

    public static CoreWindow? MainWindow => InterlockedHelper.Read(ref _mainWindow);

    public static bool HasMessageLoop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InterlockedHelper.Read(ref _isStarted) != 0;
    }

    public static bool IsMessageLoopThread
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            return messageLoopThreadId != 0 && NativeMethods.GetCurrentThreadId() == messageLoopThreadId;
        }
    }

    public static void Initialize()
    {
        uint threadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (threadId != 0)
            InvalidOperationException.Throw();
        Application.Init();
        InterlockedHelper.Write(ref _threadIdForMessageLoop, NativeMethods.GetCurrentThreadId());
        _context = GMainContext.Default;
    }

    public static void ChangeMainWindow(CoreWindow? mainWindow)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw("The message loop is not initialized!");
        ChangeMainWindowCore(mainWindow, IsMessageLoopThread);
    }

    private static void ChangeMainWindowCore(CoreWindow? mainWindow, bool isMessageLoopThread)
    {
        if (mainWindow is not null)
        {
            mainWindow.Closed += OnWindowClosed;
            if (isMessageLoopThread)
                mainWindow.ShowInternal();
            else
                InvokeAsync(_windowShowAction, mainWindow);
        }
        CoreWindow? oldWindow = InterlockedHelper.Exchange(ref _mainWindow, mainWindow);
        if (oldWindow is not null && !ReferenceEquals(oldWindow, mainWindow))
            oldWindow.Closed -= OnWindowClosed;

        static void OnWindowClosed(object? sender, EventArgs e) => Stop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Start() => Start(mainWindow: null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Start(CoreWindow? mainWindow)
    {
        if (!IsMessageLoopThread || InterlockedHelper.Exchange(ref _isStarted, 1) != 0)
            InvalidOperationException.Throw();
        ChangeMainWindowCore(mainWindow, isMessageLoopThread: true);
        int result = DoMessageLoop();
        ChangeMainWindowCore(null, isMessageLoopThread: false);
        return result;
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
        while (InterlockedHelper.Read(ref _isStarted) != 0)
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

        while (!cancellationToken.IsCancellationRequested && InterlockedHelper.Read(ref _isStarted) != 0)
            context.RunIteration(may_block: true);
    }
}
