using System.Runtime.CompilerServices;

using Gtk;

namespace ShioExtend.GtkSharp.Controls;

public sealed class Placeholder : IWidgetHolder
{
    private readonly Widget _widget;

    public Widget Widget => _widget;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Placeholder() => _widget = new DrawingArea();
}
