using System;
using System.ComponentModel;

using Gtk;

using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Windows;

public abstract class CoreWindow : Window
{
    private bool _disposed, _isInitialized;

    public event CancelEventHandler? Closing;
    public event EventHandler? Closed;

    protected CoreWindow(nint raw) : base(raw) { }

    protected CoreWindow(WindowType type) : base(type) { }

    protected CoreWindow(string title) : base(title) { }

    public new void Show()
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (!WindowMessageLoop.IsMessageLoopThread)
                InvalidOperationException.Throw();
            ShowInternal();
        }
        else
            WindowMessageLoop.Start(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use Show() insteads.")]
    public new void ShowAll() => Show();

    internal void ShowInternal()
    {
        if (!_isInitialized)
        {
            InitializeWidgets();
            _isInitialized = true;
        }
        base.ShowAll();
    }

    protected override bool OnDeleteEvent(Gdk.Event evnt)
    {
        if (base.OnDeleteEvent(evnt) || OnClosing())
            return true;

        OnClosed();
        return false;
    }

    protected abstract void InitializeWidgets();

    protected virtual bool OnClosing()
    {
        CancelEventHandler? eventHandler = Closing;
        if (eventHandler is null)
            return false;
        CancelEventArgs args = new CancelEventArgs(false);
        eventHandler.Invoke(this, args);
        return args.Cancel;
    }

    protected virtual void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);

    protected virtual void DisposeCore(bool disposing) { }

    protected override void Dispose(bool disposing)
    {
        if (ReferenceHelper.Exchange(ref _disposed, true))
            return;
        DisposeCore(disposing);
        base.Dispose(disposing);
    }
}
