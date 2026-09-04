using System;
using System.Runtime.CompilerServices;

using Gtk;

using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Controls;

public abstract partial class ScrollableElementBase : UIElement
{
    private const double ScrollBarTheresold = 10.0;

    private readonly Widget? _content;
    private readonly bool _stickBottom;

    private bool _atBottom = true;

    protected ScrollableElementBase()
    {
        ScrolledWindow window = new ScrolledWindow();

        Widget = window;
    }

    private void OnAdjustmentValueChanged(object? sender, EventArgs e)
    {
        if (sender is not Adjustment adjustment)
            return;
        double maxScroll = GetAdjustmentMaxScroll(adjustment);
        _atBottom = maxScroll <= 0 || (maxScroll - adjustment.Value) <= ScrollBarTheresold;
    }

    private void OnAdjustmentChanged(object? sender, EventArgs e)
    {
        if (sender is not Adjustment adjustment)
            return;
        if (!_atBottom)
        {
            double maxScroll = GetAdjustmentMaxScroll(adjustment);
            if (maxScroll > 0)
                return;
        }
        WindowMessageLoop.InvokeAsync(static (_this, adjustment) =>
        {
            if (!_this._atBottom)
                return;
            adjustment.Value = MathHelper.Max(GetAdjustmentMaxScroll(adjustment), 0);
        }, this, adjustment);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double GetAdjustmentMaxScroll(Adjustment adjustment) => adjustment.Upper - adjustment.PageSize;
}
