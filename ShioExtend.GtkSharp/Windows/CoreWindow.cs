using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using Gtk;

using RiceTea.Core;
using RiceTea.Core.Collections;
using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Windows;

public abstract partial class CoreWindow : NativeWindow
{
    private static readonly SyncList<GCHandle, UnwrappableList<GCHandle>> _rootWindowList = new(new());

    private readonly SyncList<GCHandle, UnwrappableList<GCHandle>> _childrenReferenceList = new(new());
    private Widget? _content;

    public Widget? Content
    {
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _content;
        }
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Widget? oldContent = _content;
            if (ReferenceEquals(Cells.Exchange(ref _content, value), value))
                return;
            Window? window = Window;
            if (window is null)
                return;
            if (oldContent is not null)
                window.Remove(oldContent);
            if (value is not null)
                window.Add(value);

            GC.KeepAlive(value);
        }
    }

    protected CoreWindow() : base(null)
    {
        _rootWindowList.Add(GCHandle.Alloc(this, GCHandleType.Weak));
    }

    protected CoreWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(passParentToUnderlyingWindow ? parent : null)
    {
        SyncList<GCHandle, UnwrappableList<GCHandle>> windowList;
        if (parent is null)
            windowList = _rootWindowList;
        else
            windowList = parent._childrenReferenceList;
        windowList.Add(GCHandle.Alloc(this, GCHandleType.Weak));
    }

    protected override void OnWindowCreated(Window window)
    {
        base.OnWindowCreated(window);

        if (_content is Widget content)
            window.Add(content);
        InitializeWidgets();
        window.ShowAll();
    }

    protected abstract void InitializeWidgets();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DisposeAndClearAllWindows() => DisposeAndClearAllWindows(_rootWindowList);

    private static void DisposeAndClearAllWindows(SyncList<GCHandle, UnwrappableList<GCHandle>> windowList)
    {
        using Lock.Scope lockScope = windowList.EnterLockScope();
        UnwrappableList<GCHandle> unwrappedList = windowList.Items;
        int count = unwrappedList.Count;
        if (count <= 0)
            return;
        ref GCHandle reference = ref UnsafeHelper.GetArrayDataReference(unwrappedList.Unwrap());
        int i = 0;
        do
        {
            ref GCHandle handle = ref UnsafeHelper.AddTypedOffset(ref reference, i);
            if (!handle.IsAllocated)
                continue;
            try
            {
                if (handle.Target is CoreWindow window)
                    window.Dispose();
            }
            finally
            {
                try
                {
                    handle.Free();
                }
                catch (Exception)
                {
                }
            }
        } while (++i < count);

        unwrappedList.Clear();
    }

    protected override void DisposeCore(bool disposing)
    {
        DisposeAndClearAllWindows(_childrenReferenceList);

        base.DisposeCore(disposing);
    }
}
