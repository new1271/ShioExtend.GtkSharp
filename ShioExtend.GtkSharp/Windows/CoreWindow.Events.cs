using System;

using Gtk;

using RiceTea.Core;

namespace ShioExtend.GtkSharp.Windows;

public delegate void ClosingEventHandler(object? sender, ref ClosingEventArgs args);

partial class CoreWindow : Window, ICheckableDisposable
{
    public event ClosingEventHandler? Closing;
    public event EventHandler? Closed;

    protected virtual void OnClosing(ref ClosingEventArgs args) => Closing?.Invoke(this, ref args);
    protected virtual void OnClosed(EventArgs args) => Closed?.Invoke(this, args);
}

public ref struct ClosingEventArgs
{
    private readonly CloseReason _reason;

    private bool _cancelled;

    public ClosingEventArgs(CloseReason reason, bool cancelled = false)
    {
        _reason = reason;
        _cancelled = cancelled;
    }

    public readonly bool Cancelled => _cancelled;

    public readonly CloseReason Reason => _reason;

    public void SetCancelled(bool cancelled) => _cancelled = cancelled;
}