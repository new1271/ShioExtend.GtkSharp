using System.Runtime.CompilerServices;

using Gtk;

using RiceTea.Core;

namespace ShioExtend.GtkSharp.Controls;

partial class ScrollableElementBase
{
    protected bool StickBottom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _stickBottom;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init
        {
            if (Cells.Exchange(ref _stickBottom, value) == value || Widget is not ScrolledWindow window)
                return;
            Adjustment adjustment = window.Vadjustment;
            if (value)
            {
                adjustment.ValueChanged += OnAdjustmentValueChanged;
                adjustment.Changed += OnAdjustmentChanged;
            }
            else
            {
                adjustment.ValueChanged -= OnAdjustmentValueChanged;
                adjustment.Changed -= OnAdjustmentChanged;
            }
        }
    }

    protected Widget Content
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _content!;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init
        {
            if (ReferenceEquals(Cells.Exchange(ref _content, value), value) || Widget is not ScrolledWindow window)
                return;
            window.Child = value;
        }
    }
}
