using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Gtk;

namespace ShioExtend.GtkSharp.Windows;

public abstract class TabbedWindow : MultiPageWindow
{
    #region Fields
    private Label? _titleLabel;
    private string _title;
    #endregion

    #region Properties
    public new string? Title
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNull]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            return _title;
        }
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            value ??= string.Empty;
            _title = value;
            _titleLabel?.Text = value;
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
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        Stack stack = base.InitializePageStack();
        Titlebar = InitializeTitleBar(stack);
        return stack;
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
            Text = _title
        });

        titleLabel.StyleContext.AddClass("title");

        _titleLabel = titleLabel;

        return headerBar;
    }
    #endregion
}
