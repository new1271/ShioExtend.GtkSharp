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

    public new void Show() => Show(forceShowAll: false);

    public new void ShowAll() => Show(forceShowAll: true);

    private void Show(bool forceShowAll)
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (!WindowMessageLoop.IsMessageLoopThread)
                InvalidOperationException.Throw();
            ShowCore(forceShowAll);
        }
        else
            WindowMessageLoop.Start(this);
    }

    internal void ShowInternal() => ShowCore(forceShowAll: false);

    private void ShowCore(bool forceShowAll)
    {
        if (!_isInitialized)
        {
            InitializeWidgets();
            _isInitialized = true;
            base.ShowAll();
        }
        else
        {
            if (forceShowAll)
                base.ShowAll();
            else
                base.Show();
        }
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
