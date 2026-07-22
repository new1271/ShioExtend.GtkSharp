using System;
using System.ComponentModel;
using System.Threading;

using Gtk;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Windows;

public abstract class CoreWindow : Window, ICheckableDisposable
{
    private bool _disposed, _isInitialized;
    private CancellationTokenSource? _dialogTokenSource;

    public event CancelEventHandler? Closing;
    public event EventHandler? Closed;

    public ResponseType Response { get; set; }

    public bool IsDisposed => _disposed;

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

    public ResponseType ShowDialog(CoreWindow? parent)
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (!WindowMessageLoop.IsMessageLoopThread)
                InvalidOperationException.Throw();
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
        return Response;
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

    private void ShowDialogCore(CoreWindow? parent)
    {
        ShowCore(forceShowAll: false);
        TransientFor = parent;
        Modal = true;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        InterlockedHelper.Write(ref _dialogTokenSource, tokenSource);
        WindowMessageLoop.StartMiniLoop(tokenSource.Token);
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
        CancellationTokenSource? dialogTokenSource = InterlockedHelper.Exchange(ref _dialogTokenSource, null);
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
        base.Dispose(disposing);
    }
}
