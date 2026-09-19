using System;

namespace ShioExtend.GtkSharp.Windows;

public delegate void ClosingEventHandler(object? sender, ref ClosingEventArgs args);

partial class NativeWindow
{
    public event EventHandler? Loaded;
    public event EventHandler? Hidden;
    public event EventHandler? Shown;
    public event EventHandler? Destroyed;
    public event ClosingEventHandler? Closing;
    public event EventHandler? Closed;

    protected virtual void OnLoaded() => Loaded?.Invoke(this, EventArgs.Empty);
    protected virtual void OnHidden() => Hidden?.Invoke(this, EventArgs.Empty);
    protected virtual void OnShown() => Shown?.Invoke(this, EventArgs.Empty);
    protected virtual void OnDestroyed() => Destroyed?.Invoke(this, EventArgs.Empty);
    protected virtual void OnClosing(ref ClosingEventArgs args) => Closing?.Invoke(this, ref args);
    protected virtual void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);
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
