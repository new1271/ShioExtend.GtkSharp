using Gtk;

namespace ShioExtend.GtkSharp.Windows;

public abstract class TabbedWindow : MultiPageWindow
{
    #region Fields
    private Label? _titleLabel;
    #endregion

    #region Constuctor       
    protected TabbedWindow() : base() { }

    protected TabbedWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(parent, passParentToUnderlyingWindow) { }
    #endregion

    #region Overrides Methods
    protected override Stack InitializePageStack()
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        Stack stack = base.InitializePageStack();
        Window!.Titlebar = InitializeTitleBar(stack);
        return stack;
    }

    protected override void ChangeTitleCore(Window window, string title)
    {
        if (_titleLabel is Label titleLabel)
            titleLabel.Text = title;
        else
            base.ChangeTitleCore(window, title);
    }
    #endregion

    #region Virtual Methods
    protected virtual HeaderBar InitializeTitleBar(Stack stack)
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

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
            Text = Title
        });

        titleLabel.StyleContext.AddClass("title");

        _titleLabel = titleLabel;

        headerBar.ShowAll();

        return headerBar;
    }
    #endregion
}
