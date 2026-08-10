using System.Runtime.CompilerServices;

using Gtk;

using ShioExtend.GtkSharp.Controls;

namespace ShioExtend.GtkSharp.Extensions;

public static class WidgetHolderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T>(this Container _this, T holder) where T : IWidgetHolder => _this.Add(holder.Widget);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove<T>(this Container _this, T holder) where T : IWidgetHolder => _this.Remove(holder.Widget);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PackStart<T>(this Box _this, T child, bool expand, bool fill, uint padding) where T : IWidgetHolder
        => _this.PackStart(child.Widget, expand, fill, padding);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PackEnd<T>(this Box _this, T child, bool expand, bool fill, uint padding) where T : IWidgetHolder
        => _this.PackEnd(child.Widget, expand, fill, padding);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReorderChild<T>(this Box _this, T child, int position) where T : IWidgetHolder
        => _this.ReorderChild(child.Widget, position);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void QueryChildPacking<T>(this Box _this, T child, out bool expand, out bool fill, out uint padding, out PackType pack_type) where T : IWidgetHolder
        => _this.QueryChildPacking(child.Widget, out expand, out fill, out padding, out pack_type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetChildPacking<T>(this Box _this, T child, bool expand, bool fill, uint padding, PackType pack_type) where T : IWidgetHolder
        => _this.SetChildPacking(child.Widget, expand, fill, padding, pack_type);
}
