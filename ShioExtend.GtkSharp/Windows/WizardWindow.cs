using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Gtk;

namespace ShioExtend.GtkSharp.Windows;

public abstract class WizardWindow : MultiPageWindow
{
    #region Fields
    private HeaderBar? _headerBar;
    private string _heading = string.Empty, _headingDescription = string.Empty;
    #endregion

    #region Properties
    public string? Heading
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNull]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _heading;
        }
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            value ??= string.Empty;
            _heading = value;
            _headerBar?.Title = value;
        }
    }

    public string? HeadingDescription
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNull]
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _headingDescription;
        }
        protected set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            value ??= string.Empty;
            _headingDescription = value;
            _headerBar?.Subtitle = value;
        }
    }
    #endregion

    #region Constuctor       
    protected WizardWindow(nint raw) : base(raw) { }

    protected WizardWindow(WindowType type) : base(type) { }

    protected WizardWindow(string title) : base(title) { }
    #endregion

    #region Overrides Methods
    protected override Stack InitializePageStack()
    {
        Stack stack = base.InitializePageStack();
        Titlebar = InitializeTitleBar(stack);
        return stack;
    }
    #endregion

    #region Virtual Methods
    protected virtual HeaderBar InitializeTitleBar(Stack stack)
    {
        HeaderBar headerBar = new HeaderBar()
        {
            Title = _heading,
            Subtitle = _headingDescription,
            ShowCloseButton = false
        };
        _headerBar = headerBar;
        return headerBar;
    }
    #endregion
}
