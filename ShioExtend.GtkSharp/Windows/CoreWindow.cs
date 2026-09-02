using System;
using System.Threading;

using Gtk;

using RiceTea.Core;

namespace ShioExtend.GtkSharp.Windows;

public abstract partial class CoreWindow : Window, ICheckableDisposable
{
    private CancellationTokenSource? _dialogTokenSource;
    private CloseReason _closeReason;
    private bool _disposed, _isInitialized;

    public DialogResult DialogResult { get; set; }

    public bool IsDisposed => _disposed;

    protected CoreWindow(nint raw) : base(raw) => WindowMessageLoop.ThrowIfNotInMessageLoopThread();

    protected CoreWindow(WindowType type) : base(type) => WindowMessageLoop.ThrowIfNotInMessageLoopThread();

    protected CoreWindow(string title) : base(title) => WindowMessageLoop.ThrowIfNotInMessageLoopThread();

    public new void Show() => Show(forceShowAll: false);

    public new void ShowAll() => Show(forceShowAll: true);

    private void Show(bool forceShowAll)
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        if (WindowMessageLoop.HasMessageLoop)
            ShowCore(forceShowAll);
        else
            WindowMessageLoop.Start(this);
    }

    public DialogResult ShowDialog(CoreWindow? parent)
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        if (WindowMessageLoop.HasMessageLoop)
        {
            ShowDialogCore(parent);
        }
        else
        {
            if (parent is null)
                WindowMessageLoop.Start(this);
            else
            {
                parent.Show();
                if (!WindowMessageLoop.HasMessageLoop || !WindowMessageLoop.IsMessageLoopThread)
                    InvalidOperationException.Throw();
                ShowDialogCore(parent);
            }
        }
        return DialogResult;
    }

    internal void ShowInternal() => ShowCore(forceShowAll: false);

    private void ShowCore(bool forceShowAll)
    {
        if (!_isInitialized)
        {
            InitializeWidgets();
            _isInitialized = true;
            OnLoaded();
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

    private void ShowDialogCore(CoreWindow? parent)
    {
        ShowCore(forceShowAll: false);
        TransientFor = parent;
        Modal = true;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Atomics.Write(ref _dialogTokenSource, tokenSource);
        WindowMessageLoop.StartMiniLoop(tokenSource.Token);
    }

    public new void Close() => Close(CloseReason.Programmically);

    public void Close(CloseReason reason)
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        _closeReason = reason;
        base.Close();
    }

    protected override bool OnDeleteEvent(Gdk.Event evnt)
    {
        if (base.OnDeleteEvent(evnt))
            return true;
        ClosingEventArgs args = new ClosingEventArgs(Cells.Exchange(ref _closeReason, CloseReason.UserClicked), cancelled: false);
        OnClosing(ref args);
        if (args.Cancelled)
            return true;

        OnClosed();
        return false;
    }

    protected override void OnDestroyed()
    {
        CancellationTokenSource? dialogTokenSource = Atomics.Exchange(ref _dialogTokenSource, null);
        if (dialogTokenSource is not null)
        {
            try
            {
                dialogTokenSource.Cancel(throwOnFirstException: false);
            }
            catch (Exception)
            {
            }
            finally
            {
                dialogTokenSource.Dispose();
            }
        }
        base.OnDestroyed();
    }

    protected abstract void InitializeWidgets();

    protected virtual void DisposeCore(bool disposing) { }

    protected override void Dispose(bool disposing)
    {
        if (Cells.Exchange(ref _disposed, true))
            return;
        DisposeCore(disposing);
        base.Dispose(disposing);
    }
}
