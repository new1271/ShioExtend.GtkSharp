using System;
using System.Runtime.CompilerServices;

using Gtk;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

using ShioExtend.GtkSharp.Windows;

namespace ShioExtend.GtkSharp;

internal static partial class WindowMessageLoop
{
    private static readonly Action<int> _stopAction = static exitCode =>
    {
        _exitCode = exitCode;
        Application.Quit();
    };
    private static readonly Action<CoreWindow> _windowShowAction = static window => window.ShowAll();

    private static CoreWindow? _mainWindow;
    private static uint _invokeBarrier, _threadIdForMessageLoop;
    private static int _exitCode;

    public static CoreWindow? MainWindow => InterlockedHelper.Read(ref _mainWindow);

    public static bool IsMessageLoopThread
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            return messageLoopThreadId != 0 && NativeMethods.GetCurrentThreadId() == messageLoopThreadId;
        }
    }

    public static int Start(CoreWindow mainWindow)
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

    private static void ChangeMainWindowCore(CoreWindow? mainWindow, bool isMessageLoopThread)
    {
        if (mainWindow is not null)
        {
            mainWindow.Closed += OnWindowClosed;
            if (isMessageLoopThread)
                mainWindow.ShowAll();
            else
                InvokeAsync(_windowShowAction, mainWindow);
        }
        CoreWindow? oldWindow = InterlockedHelper.Exchange(ref _mainWindow, mainWindow);
        if (oldWindow is not null)
            oldWindow.Closed -= OnWindowClosed;

        static void OnWindowClosed(object? sender, EventArgs e) => Stop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Stop(int exitCode = 0) => InvokeAsync(_stopAction, exitCode);
}
