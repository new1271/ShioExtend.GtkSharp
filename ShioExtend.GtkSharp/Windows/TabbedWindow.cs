using GLib;

using Gtk;

namespace ShioExtend.GtkSharp.Windows;

public abstract class TabbedWindow : MultiPageWindow
{
    #region Fields
    private Label _titleLabel = null!;
    #endregion

    #region Constuctor       
    protected TabbedWindow(nint raw) : base(raw) { }

    protected TabbedWindow(WindowType type) : base(type) { }

    protected TabbedWindow(string title) : base(title) { }
    #endregion

    #region Overrides Methods
    protected override Stack InitializePageStack()
    {
        Stack stack = base.InitializePageStack();
        InitializeTitleBar(stack);
        return stack;
    }

    protected override void InitializeWidgets()
    {
        base.InitializeWidgets();
        AddNotification("title", OnTitleChanged);
    }

    protected override void OnClosed()
    {
        RemoveNotification("title", OnTitleChanged);
        base.OnClosed();
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
        headerBar.PackStart(_titleLabel = new Label()
        {
            MarginStart = UIConstants.WidgetMargin,
            MarginEnd = UIConstants.WidgetMargin,
        });

        UpdateTitleLabel(Title);
        Titlebar = headerBar;
    }

    private static void OnTitleChanged(object sender, NotifyArgs e)
    {
        if (sender is not TabbedWindow window)
            return;
        window.UpdateTitleLabel(window.Title);
    }

    private void UpdateTitleLabel(string text) => _titleLabel.Markup = $"<span weight=\"bold\" size=\"larger\">{text}</span>";
    #endregion
}
