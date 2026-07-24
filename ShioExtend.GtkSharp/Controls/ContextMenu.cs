using System.Collections.Generic;

using Gtk;

using RiceTea.Core.Helpers;

namespace ShioExtend.GtkSharp.Controls;

public static partial class ContextMenu
{
    public static void Open(Item[] items, Gdk.Event @event)
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        int length = items.Length;
        if (length <= 0)
            return;

        Menu menu = new Menu();

        ref readonly Item itemsRef = ref UnsafeHelper.GetArrayDataReference(items);
        int i = 0;
        do
        {
            menu.Append(UnsafeHelper.AddTypedOffsetAsReadOnly(in itemsRef, i).ToMenuItem());
        } while (++i < length);
        menu.ShowAll();

        menu.PopupAtPointer(@event);
    }

    public static void Open<TEnumerable>(TEnumerable items, Gdk.Event @event) where TEnumerable : IEnumerable<Item>
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        Menu menu;
        using (IEnumerator<Item> enumerator = items.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                return;

            menu = new Menu();
            do
            {
                menu.Append(enumerator.Current.ToMenuItem());
            } while (enumerator.MoveNext());
        }
        menu.ShowAll();

        menu.PopupAtPointer(@event);
    }
}
