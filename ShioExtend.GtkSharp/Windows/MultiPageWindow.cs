using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using GLib;

using Gtk;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Windows;

public abstract class MultiPageWindow : CoreWindow
{
    #region Static Fields
    [ThreadStatic]
    private static PooledList<string>? _initOnlyPageList;
    [ThreadStatic]
    private static bool _ignorePageChangeNotification;
    #endregion

    #region Fields
    private string[] _pageNames = null!;
    private Stack _pageStack = null!;
    private uint _pageIndex, _pageCount;
    #endregion

    #region Properties
    public uint PageCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pageCount;
    }

    public uint CurrentPage
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pageIndex;
        set
        {
            if (_pageIndex == value)
                return;
            if (!TryQueryPageName(value, out string? pageName))
            {
                ArgumentOutOfRangeException.Throw(nameof(value));
                return;
            }
            _ignorePageChangeNotification = true;
            try
            {
                _pageStack.VisibleChildName = pageName;
            }
            finally
            {
                _ignorePageChangeNotification = false;
            }
            OnCurrentPageChanging();
            _pageIndex = value;
            OnCurrentPageChanged();
        }
    }
    #endregion

    #region Events
    public event EventHandler? CurrentPageChanging;
    public event EventHandler? CurrentPageChanged;
    #endregion

    #region Event Triggers
    protected virtual void OnCurrentPageChanging()
    {
        CurrentPageChanging?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnCurrentPageChanged()
    {
        CurrentPageChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Constuctor       
    protected MultiPageWindow(nint raw) : base(raw) { }

    protected MultiPageWindow(WindowType type) : base(type) { }

    protected MultiPageWindow(string title) : base(title) { }
    #endregion

    #region Override Methods
    protected override void InitializeWidgets()
    {
        Stack stack = InitializePageStack();
        Add(stack);
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

        stack.AddNotification("visible-child-name", PageStack_VisibleChildNameChanged);
    }

    protected override void OnClosed()
    {
        _pageStack.RemoveNotification("visible-child-name", PageStack_VisibleChildNameChanged);
        base.OnClosed();
    }
    #endregion

    #region Virtual Methods
    protected virtual Stack InitializePageStack()
        => new Stack()
        {
            /*
            TransitionType = StackTransitionType.SlideLeftRight,
            TransitionDuration = 200
            */
        };

    protected virtual void AppendPage(Widget widget, string name)
    {
        PooledList<string>? list = _initOnlyPageList;
        if (list is null)
            InvalidOperationException.Throw($"{nameof(AppendPage)} is initialize-only!");
        _pageStack.AddNamed(widget, name);
        list.Add(name);
    }

    protected virtual void AppendPage(Widget widget, string name, string title)
    {
        PooledList<string>? list = _initOnlyPageList;
        if (list is null)
            InvalidOperationException.Throw($"{nameof(AppendPage)} is initialize-only!");
        _pageStack.AddTitled(widget, name, title);
        list.Add(name);
    }
    #endregion

    #region Abstract Methods
    protected abstract void InitializePages();
    #endregion

    #region Normal Methods
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
        if (_ignorePageChangeNotification || sender is not Stack stack)
            return;
        string? name = stack.VisibleChildName;
        if (name is null || !TryQueryPageIndex(name, out uint result))
        {
            InvalidOperationException.Throw();
            return;
        }
        OnCurrentPageChanging();
        _pageIndex = result;
        OnCurrentPageChanged();
    }
    #endregion
}
