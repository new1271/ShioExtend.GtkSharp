using System.Runtime.CompilerServices;

using Gtk;

namespace ShioExtend.GtkSharp.Controls;

public sealed class Placeholder : UIElement
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Placeholder() => Widget = new DrawingArea();
}
