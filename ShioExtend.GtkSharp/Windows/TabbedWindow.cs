using System;

using Atk;

using GLib;

using Gtk;

using RiceTea.Core;

namespace ShioExtend.GtkSharp.Windows;

public abstract class TabbedWindow : MultiPageWindow
{
    #region Fields
    private Label _titleLabel = null!;
    private string _title;
    #endregion

    #region Properties
    public new string Title
    {
        get => Atomics.Read(ref _title);
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            UpdateTitleLabel(value);
        }
    }
    #endregion

    #region Constuctor       
    protected TabbedWindow(nint raw) : base(raw) => _title = string.Empty;

    protected TabbedWindow(WindowType type) : base(type) => _title = string.Empty;

    protected TabbedWindow(string title) : base(title) => _title = title;
    #endregion

    #region Overrides Methods
    protected override Stack InitializePageStack()
    {
        Stack stack = base.InitializePageStack();
        InitializeTitleBar(stack);
        return stack;
    }
    #endregion

    #region Normal Methods
    private void InitializeTitleBar(Stack stack)
    {
        HeaderBar headerBar = new HeaderBar()
        {
            CustomTitle = new StackSwitcher()
            {
                Stack = stack,
            },
            ShowCloseButton = true
        };

        Label titleLabel;
        headerBar.PackStart(titleLabel = new Label()
        {
            MarginStart = UIConstants.WidgetMargin,
            MarginEnd = UIConstants.WidgetMargin,
            Text = _title
        });

        titleLabel.StyleContext.AddClass("title");

        _titleLabel = titleLabel;
        Titlebar = headerBar;
    }

    private void UpdateTitleLabel(string text)
    {
        Atomics.Write(ref _title, text);
        _titleLabel?.Text = text;
    }
    #endregion
}
