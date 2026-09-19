using System;

using Gtk;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Windows;

partial class NativeWindow : ICheckableDisposable
#if NET8_0_OR_GREATER
    , IAsyncDisposable
#endif
{
    private Window CreateWindow(Window? parent)
    {
        CreateWindowInfo windowInfo = GetCreateWindowInfo();
        Window result = new Window(WindowType.Toplevel);
        if (parent is not null)
            result.TransientFor = parent;

        int x = windowInfo.X, y = windowInfo.Y, width = windowInfo.Width, height = windowInfo.Height;
        if (x == CreateWindowInfo.UseDefaultSizeOrLocation)
        {
            if (y != CreateWindowInfo.UseDefaultSizeOrLocation)
            {
                result.GetPosition(out x, out _);
                result.Move(x, y);
            }
        }
        else
        {
            if (y == CreateWindowInfo.UseDefaultSizeOrLocation)
                result.GetPosition(out _, out y);
            result.Move(x, y);
        }
        if (width == CreateWindowInfo.UseDefaultSizeOrLocation)
        {
            if (height != CreateWindowInfo.UseDefaultSizeOrLocation)
                result.SetDefaultSize(-1, height);
        }
        else
        {
            result.SetDefaultSize(width, height == CreateWindowInfo.UseDefaultSizeOrLocation ? -1 : height);
        }

        RuntimeFlags = GetRuntimeFlagsDirectly() | WindowRuntimeFlags.Initialized;
        return result;
    }

    protected virtual CreateWindowInfo GetCreateWindowInfo() => new CreateWindowInfo();

    protected virtual void OnWindowCreated(Window window)
    {
        string title = Atomics.CompareExchange(ref _tempTitle, nameof(NativeWindow), null) ?? nameof(NativeWindow);
        Gdk.Pixbuf? icon = Atomics.Read(ref _tempIcon);

        window.Title = title;
        if (icon is not null)
            window.Icon = icon;
    }

    private void DisposeInternal(bool disposing)
    {
        if (Atomics.Exchange(ref _disposed, UnsafeHelper.GetMaxValue<nuint>()) != 0 ||
            WindowMessageLoop.TryInvoke(static (_this, disposing) => _this.DisposeSync(disposing), this, disposing))
            return;
        DisposeSync(disposing);
    }

    private void DisposeSync(bool disposing)
    {
        try
        {
            DisposeCore(disposing);
        }
        finally
        {
            Window? window = _window;
            if (window is not null)
            {
                Atomics.Write(ref _window, null);
                window.Destroy();
            }
            RuntimeFlags = WindowRuntimeFlags.Destroyed;
        }
    }

    protected virtual void DisposeCore(bool disposing) => _window?.Dispose();

    ~NativeWindow() => DisposeInternal(disposing: false);

    public void Dispose()
    {
        DisposeInternal(disposing: true);
        GC.SuppressFinalize(this);
    }

#if NET8_0_OR_GREATER
    private System.Threading.Tasks.Task DisposeInternalAsync()
    {
        if (Atomics.Exchange(ref _disposed, UnsafeHelper.GetMaxValue<nuint>()) != 0)
            goto Tail;
        System.Threading.Tasks.Task? task = WindowMessageLoop.TryInvokeTaskAsync(static _this => _this.DisposeSync(disposing: true), this);
        if (task is not null)
            return task;

        DisposeSync(disposing: true);

    Tail:
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async System.Threading.Tasks.ValueTask DisposeAsync()
    {
        await DisposeInternalAsync();
        GC.SuppressFinalize(this);
    }
#endif
}
