using System;
using System.Runtime.CompilerServices;

using Gtk;

namespace ShioExtend.GtkSharp.Controls;

partial class ContextMenu
{
    public sealed class Item
    {
        private readonly string _text;
        private EventHandler? _click;

        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _text;
        }

        public event EventHandler? Click
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => _click += value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => _click -= value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Item(string text) => _text = text;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MenuItem ToMenuItem()
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return ToMenuItemCore();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MenuItem ToMenuItemCore()
        {
            MenuItem result = new MenuItem(_text);
            EventHandler? click = _click;
            if (click is not null)
                result.Activated += click;
            return result;
        }
    }
}
