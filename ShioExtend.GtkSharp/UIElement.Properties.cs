using System.Runtime.CompilerServices;

using Gtk;

namespace ShioExtend.GtkSharp;

partial class UIElement
{
    public Widget Widget
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _widget;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected init => _widget = value;
    }

    public bool Sensitive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _widget.Sensitive;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            _widget.Sensitive = value;
        }
    }

    public bool Visible
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _widget.Visible;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            _widget.Visible = value;
        }
    }

    public bool NoShowAll
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _widget.NoShowAll;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            _widget.NoShowAll = value;
        }
    }

    public StyleContext StyleContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _widget.StyleContext;
        }
    }

    public int Margin
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            return _widget.Margin;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            _widget.Margin = value;
        }
    }

    public int MarginStart
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            return _widget.MarginStart;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            _widget.MarginStart = value;
        }
    }

    public int MarginEnd
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            return _widget.MarginEnd;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            _widget.MarginEnd = value;
        }
    }

    public int MarginTop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            return _widget.MarginTop;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            _widget.MarginTop = value;
        }
    }

    public int MarginBottom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            return _widget.MarginBottom;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            _widget.MarginBottom = value;
        }
    }
}
