using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using GLib;

using Gtk;

using RiceTea.Core;
using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Windows;

public abstract partial class MultiPageWindow : CoreWindow
{
    #region Static Fields
    [ThreadStatic]
    private static PooledList<string>? _initOnlyPageList;
    #endregion

    #region Fields
    private string[] _pageNames = null!;
    private Stack? _pageStack;
    private uint _pageIndex, _pageCount;
    private bool _isLoaded;
    #endregion

    #region Properties
    public uint PageCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _pageCount;
        }
    }

    public uint CurrentPage
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _pageIndex;
        }
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            uint oldPageIndex = Cells.Exchange(ref _pageIndex, value);
            if (oldPageIndex == value || !_isLoaded)
                return;
            if (!TryQueryPageName(value, out string? pageName))
            {
                ArgumentOutOfRangeException.Throw(nameof(value));
                return;
            }
            _pageStack?.VisibleChildName = pageName;
        }
    }
    #endregion

    #region Constuctor       
    protected MultiPageWindow() : base() { }

    protected MultiPageWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(parent, passParentToUnderlyingWindow) { }
    #endregion

    #region Override Methods
    protected override void InitializeWidgets()
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        Stack stack = InitializePageStack();
        Content = stack;
        _pageStack = stack;

        PooledList<string> list = new();
        _initOnlyPageList = list;
        try
        {
            InitializePages();

            string[] pageNames = list.ToArray();
            DebugHelper.ThrowIf(pageNames.Length < 0);
            _pageNames = pageNames;
            _pageCount = (uint)pageNames.Length;
        }
        finally
        {
            _initOnlyPageList = null;
            list.Dispose();
        }
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        if (!WindowMessageLoop.IsMessageLoopThread) // ShioExtend 觸發的 OnLoaded 必定在視窗訊息執行緒
            return;

        _pageStack!.VisibleChildName = _pageNames[_pageIndex];
        WindowMessageLoop.InvokeAsync(static _this => _this.OnLoaded_RunLater(), this); // 脫離目前上下文後再執行，避免被使用者程式碼影響
    }

    protected override void OnClosed()
    {
        _pageStack!.RemoveNotification("visible-child-name", PageStack_VisibleChildNameChanged);
        base.OnClosed();
    }
    #endregion

    #region Virtual Methods
    protected virtual Stack InitializePageStack()
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        return new Stack()
        {
            /*
            TransitionType = StackTransitionType.SlideLeftRight,
            TransitionDuration = 200
            */
        };
    }

    protected virtual void AppendPage(Widget widget, string name)
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        PooledList<string>? list = _initOnlyPageList;
        if (list is null)
            InvalidOperationException.Throw($"{nameof(AppendPage)} is initialize-only!");
        _pageStack!.AddNamed(widget, name);
        widget.ShowAll();
        list.Add(name);
    }

    protected virtual void AppendPage(Widget widget, string name, string title)
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        PooledList<string>? list = _initOnlyPageList;
        if (list is null)
            InvalidOperationException.Throw($"{nameof(AppendPage)} is initialize-only!");
        _pageStack!.AddTitled(widget, name, title);
        widget.ShowAll();
        list.Add(name);
    }
    #endregion

    #region Abstract Methods
    protected abstract void InitializePages();
    #endregion

    #region Normal Methods
    private void OnLoaded_RunLater()
    {
        Stack? stack = _pageStack;
        DebugHelper.ThrowIf(stack is null);
        stack.AddNotification("visible-child-name", PageStack_VisibleChildNameChanged);

        _isLoaded = true;
        PageStack_VisibleChildNameChanged(stack);
    }

    private bool TryQueryPageName(uint pageIndex, [NotNullWhen(true)] out string? result)
    {
        if (pageIndex >= _pageCount)
        {
            result = null;
            return false;
        }
        result = _pageNames.AsUnsafeRef()[pageIndex];
        return true;
    }

    private bool TryQueryPageIndex(string pageName, out uint result)
    {
        int index = _pageNames.IndexOf(pageName);
        if (index < 0)
            goto Failed;
        result = (uint)index;
        return result < _pageCount;

    Failed:
        result = default;
        return false;
    }

    private void PageStack_VisibleChildNameChanged(object sender, NotifyArgs e)
    {
        if (sender is not Stack stack)
            return;
        PageStack_VisibleChildNameChanged(stack);
    }

    private void PageStack_VisibleChildNameChanged(Stack stack)
    {
        string? name = stack.VisibleChildName;
        if (name is null || !TryQueryPageIndex(name, out uint result))
        {
            InvalidOperationException.Throw();
            return;
        }
        uint oldPageIndex = Cells.Exchange(ref _pageIndex, result);
        OnCurrentPageChanged(new CurrentPageChangedEventArgs(oldPageIndex, result));
    }
    #endregion
}
