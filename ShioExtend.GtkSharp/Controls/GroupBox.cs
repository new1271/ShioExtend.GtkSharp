using System.Diagnostics.CodeAnalysis;

using Gtk;

using ShioExtend.GtkSharp.Internals;

namespace ShioExtend.GtkSharp.Controls;

public sealed class GroupBox : UIElement
{
    private readonly Label _titleLabel, _titleDescriptionLabel;
    private readonly Box _contentBox, _titleBox;
    private readonly Placeholder _contentPlaceholder;

    private Widget? _content;

    [AllowNull]
    public string Title
    {
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _titleLabel.Text ?? string.Empty;
        }
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Label titleLabel = _titleLabel;
            titleLabel.Text = value;

            Box titleBox = _titleBox;
            if (string.IsNullOrEmpty(value))
            {
                titleBox.Visible = _titleDescriptionLabel.Visible;
                titleLabel.Visible = false;
            }
            else
            {
                titleBox.Visible = true;
                titleLabel.Visible = true;
            }
        }
    }

    public string TitleDescription
    {
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            return _titleDescriptionLabel.Text;
        }
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Label titleDescriptionLabel = _titleDescriptionLabel;
            titleDescriptionLabel.Text = value;

            Box titleBox = _titleBox;
            if (string.IsNullOrEmpty(value))
            {
                titleBox.Visible = _titleLabel.Visible;
                titleDescriptionLabel.Visible = false;
            }
            else
            {
                titleBox.Visible = true;
                titleDescriptionLabel.Visible = true;
            }
        }
    }

    public Widget? Content
    {
        get
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();
            return _content;
        }
        set
        {
            WindowMessageLoop.ThrowIfNotInMessageLoopThread();

            Widget? content = _content;
            if (ReferenceEquals(content, value))
                return;

            Box box = _contentBox;
            Placeholder placeholder = _contentPlaceholder;
            if (content is null)
                box.Remove(placeholder);
            else
                box.Remove(content);

            if (value is null)
                box.PackEnd(placeholder, expand: true, fill: true, padding: 0);
            else
                box.PackEnd(value, expand: true, fill: true, padding: 0);

            _content = value;
        }
    }

    public GroupBox()
    {
        Frame frame = new Frame() { ShadowType = ShadowType.None };
        Box box = new Box(Orientation.Vertical, UIConstants.WidgetMargin)
        {
            MarginStart = UIConstants.WidgetMargin,
            MarginEnd = UIConstants.WidgetMargin,
            MarginTop = UIConstants.WidgetMarginSmall,
            MarginBottom = UIConstants.WidgetMargin
        };

        Label titleLabel = new Label() { Visible = false, NoShowAll = true };
        titleLabel.StyleContext.AddClass("heading");

        Box titleBox = new Box(Orientation.Horizontal, UIConstants.WidgetMarginTiny) { Visible = false, NoShowAll = true };
        titleBox.PackStart(titleLabel, expand: false, fill: true, padding: 0);
        Label titleDescriptionLabel = new Label() { Visible = false, NoShowAll = true, Valign = Align.End };
        StyleContext styleContext = titleDescriptionLabel.StyleContext;
        styleContext.AddClass("dim-label");
        styleContext.AddClass("caption-heading");
        titleBox.PackStart(titleDescriptionLabel, expand: false, fill: true, padding: 0);
        titleBox.PackStart(new Label(), expand: true, fill: true, padding: 0);
        box.PackStart(titleBox, expand: false, fill: true, padding: 0);

        Placeholder contentPlaceholder = new Placeholder() { Visible = true, NoShowAll = true };
        box.PackStart(contentPlaceholder, expand: true, fill: true, padding: 0);
        frame.Add(box);
        frame.StyleContext.AddProvider(StyleProviders.FrameCardStyleProvider, StyleProviderPriority.Application);

        _titleLabel = titleLabel;
        _titleDescriptionLabel = titleDescriptionLabel;
        _titleBox = titleBox;
        _contentBox = box;
        _contentPlaceholder = contentPlaceholder;
        Widget = frame;
    }
}
