using System;
using System.Runtime.CompilerServices;

using Gtk;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

using ShioExtend.GtkSharp.Windows;

namespace ShioExtend.GtkSharp;

public static partial class WindowMessageLoop
{
    private static readonly Action<int> _stopAction = static exitCode =>
    {
        _exitCode = exitCode;
        Application.Quit();
    };
    private static readonly Action<CoreWindow> _windowShowAction = static window => window.ShowInternal();

    private static CoreWindow? _mainWindow;
    private static uint _invokeBarrier, _threadIdForMessageLoop;
    private static int _exitCode;

    public static CoreWindow? MainWindow => InterlockedHelper.Read(ref _mainWindow);

    public static bool HasMessageLoop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            return messageLoopThreadId != 0;
        }
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

    public static void ChangeMainWindow(CoreWindow? mainWindow)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw("The message loop is not exists!");
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
        uint currentThreadId = NativeMethods.GetCurrentThreadId();
        if (InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, currentThreadId, 0) != 0)
            InvalidOperationException.Throw("Message loop is already exists!");

        ChangeMainWindowCore(mainWindow, isMessageLoopThread: true);
        Application.Run();
        int result = _exitCode;
        InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, 0, currentThreadId);

        ChangeMainWindowCore(null, isMessageLoopThread: false);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Stop(int exitCode = 0) => InvokeAsync(_stopAction, exitCode);
}
