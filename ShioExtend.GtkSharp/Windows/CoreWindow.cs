using System;
using System.ComponentModel;

using Gtk;

using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Windows;

public abstract class CoreWindow : Window
{
    private bool _disposed;

    public event CancelEventHandler? Closing;
    public event EventHandler? Closed;

    protected CoreWindow(nint raw) : base(raw) { }

    protected CoreWindow(WindowType type) : base(type) { }

    protected CoreWindow(string title) : base(title) { }

    protected override bool OnDeleteEvent(Gdk.Event evnt)
    {
        if (base.OnDeleteEvent(evnt) || OnClosing())
            return true;

        OnClosed();
        return false;
    }

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
