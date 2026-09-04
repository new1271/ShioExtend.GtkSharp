using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Gtk;

using RiceTea.Core.Collections;

namespace ShioExtend.GtkSharp.Controls;

public sealed class DropdownBox : UIElement
{
    private static readonly IStyleProvider ListBoxStyleProvider = CreateListBoxStyleProvider();

    private static CssProvider CreateListBoxStyleProvider()
    {
        CssProvider cssProvider = new CssProvider();
        cssProvider.LoadFromData("""
        list  {
            background-color: transparent;
        }
        """);
        return cssProvider;
    }

    private readonly ScrolledWindow _scrolledWindow;
    private readonly Label _label;
    private readonly Popover _menu;
    private readonly ListBox _listBox;
    private readonly ObservableList<string> _items;

    public event EventHandler? ItemClicked;

    public IList<string> Items
    {
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _items;
        }
    }

    public int SelectedIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _listBox.SelectedRow?.Index ?? -1;
        }
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            ListBox listBox = _listBox;
            ListBoxRow? row = listBox.GetRowAtIndex(value);
            listBox.SelectRow(row);
            _label.Text = (row?.Child as Label)?.Text;
        }
    }

    public bool Enabled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return Widget.Sensitive;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Widget.Sensitive = value;
        }
    }

    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _label.Text;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            _label.Text = value;
        }
    }

    public DropdownBox()
    {
        Label label = new Label() { Xalign = 0 };
        Box box = new Box(Orientation.Horizontal, UIConstants.WidgetMarginTiny);
        MenuButton button = new MenuButton();
        box.PackStart(label, expand: true, fill: true, padding: 0);
        box.PackEnd(Image.NewFromIconName("pan-down-symbolic", IconSize.Button), expand: false, fill: true, padding: 0);
        button.Add(box);

        PopoverMenu menu = new PopoverMenu()
        {
            Position = PositionType.Bottom,
            ConstrainTo = PopoverConstraint.Window,
            Modal = true,
        };
        ListBox listBox = new ListBox()
        {
            SelectionMode = SelectionMode.Single,
            Margin = UIConstants.WidgetMargin,
        };
        listBox.StyleContext.AddProvider(ListBoxStyleProvider, StyleProviderPriority.Application);

        ScrolledWindow window = new ScrolledWindow()
        {
            HscrollbarPolicy = PolicyType.Never,
            VscrollbarPolicy = PolicyType.Automatic,
            PropagateNaturalHeight = true,
            PropagateNaturalWidth = false,
        };
        window.Add(listBox);
        menu.Add(window);

        button.ShowAll();
        window.ShowAll();

        button.Popover = menu;

        listBox.ListRowActivated += ListBox_ListRowActivated;

        ObservableList<string> items = new ObservableList<string>();
        items.BeforeAdd += Items_BeforeAdd;
        items.BeforeClear += Items_BeforeClear;
        items.BeforeInsert += Items_BeforeInsert;
        items.BeforeModify += Items_BeforeModify;
        items.BeforeRemove += Items_BeforeRemove;
        items.BeforeRemoveAt += Items_BeforeRemoveAt;
        label.SizeAllocated += Label_SizeAllocated;
        button.SizeAllocated += Button_SizeAllocated;

        Widget = button;
        _scrolledWindow = window;
        _label = label;
        _menu = menu;
        _listBox = listBox;
        _items = items;
    }

    private void Button_SizeAllocated(object o, SizeAllocatedArgs args)
    {
        _menu.SetSizeRequest(args.Allocation.Width, -1);
    }

    private void Label_SizeAllocated(object o, SizeAllocatedArgs args)
    {
        _scrolledWindow.MaxContentHeight = args.Allocation.Height * 5 + UIConstants.WidgetMargin;
    }

    private void ListBox_ListRowActivated(object o, ListRowActivatedArgs args)
    {
        _menu.Popdown();
        if (args.Row.Child is Label label)
            _label.Text = label.Text;
        ItemClicked?.Invoke(this, EventArgs.Empty);
    }

    private static Label CreateItemLabel(string text)
    {
        Label label = new Label(text) { Xalign = 0 };
        label.ShowAll();
        return label;
    }

    private void Items_BeforeAdd(object? sender, BeforeListAddOrRemoveEventArgs<string> args)
        => _listBox.Insert(CreateItemLabel(args.Item), -1);

    private void Items_BeforeInsert(object? sender, BeforeListModifyEventArgs<string> args)
        => _listBox.Insert(CreateItemLabel(args.Item), args.Index);

    private void Items_BeforeModify(object? sender, BeforeListModifyEventArgs<string> args)
    {
        ListBoxRow? row = _listBox.GetRowAtIndex(args.Index);
        if (row is null)
            return;
        if (row.Child is Label label)
            label.Text = args.Item;
        else
            row.Child = new Label(args.Item);
    }

    private void Items_BeforeClear(object? sender, CancelEventArgs args)
    {
        ListBox listBox = _listBox;
        foreach (Widget child in listBox.Children)
            child.Destroy();
        _label.Text = string.Empty;
    }

    private void Items_BeforeRemove(object? sender, BeforeListAddOrRemoveEventArgs<string> args)
    {
        if (sender is not ObservableList<string> list)
            return;
        ListBox listBox = _listBox;
        ListBoxRow? row = listBox.GetRowAtIndex(list.IndexOf(args.Item));
        if (row is null)
            return;
        if (row.Handle == listBox.SelectedRow.Handle)
            _label.Text = string.Empty;
        row.Destroy();
    }

    private void Items_BeforeRemoveAt(object? sender, BeforeListModifyEventArgs<string> args)
    {
        ListBox listBox = _listBox;
        ListBoxRow? row = listBox.GetRowAtIndex(args.Index);
        if (row is null)
            return;
        if (row.Handle == listBox.SelectedRow.Handle)
            _label.Text = string.Empty;
        row.Destroy();
    }
}
