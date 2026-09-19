using System.Runtime.CompilerServices;

using Gtk;

using ShioExtend.GtkSharp.Controls;

namespace ShioExtend.GtkSharp;

public abstract partial class UIElement
{
    private readonly Widget _widget = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected UIElement() => WindowMessageLoop.ThrowIfNotInMessageLoopThread();

    public void Destroy()
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();
        _widget.Destroy();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Widget(UIElement element) => element.Widget;
}
