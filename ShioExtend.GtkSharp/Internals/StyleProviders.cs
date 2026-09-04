using Gtk;

namespace ShioExtend.GtkSharp.Internals;

internal static class StyleProviders
{
    public static readonly CssProvider FrameCardStyleProvider = CreateFrameCardStyleProvider();
    public static readonly CssProvider ScreenWideFallbackStyleProvider = CreateScreenWideFallbackStyleProvider();

    private static CssProvider CreateFrameCardStyleProvider()
    {
        CssProvider provider = new CssProvider();
        provider.LoadFromData("""
            frame {
                background-color: @theme_base_color;
            """ + $"""
                border-radius: {UIConstants.WidgetMarginSmall}px;
            """ + """
                border: 1px solid alpha(@theme_fg_color, 0.12);
                box-shadow: 0 1px 2px alpha(#000000, 0.08);
            }
            """);
        return provider;
    }

    private static CssProvider CreateScreenWideFallbackStyleProvider()
    {
        CssProvider provider = new CssProvider();
        provider.LoadFromData("""
        .title-1 {
            font-size: 1.6rem;
            font-weight: 800;
        }

        .title-2 {
            font-size: 1.3rem;
            font-weight: bold;
        }

        .title-3 {
            font-size: 1.15rem;
            font-weight: bold;
        }

        .title-4 {
            font-size: 1.05rem;
            font-weight: bold;
        }

        .heading {
            font-weight: bold;
        }

        .caption-heading {
            font-size: 0.85rem;
            font-weight: bold;
        }

        .caption {
            font-size: 0.85rem;
            opacity: 0.65;
        }

        .body {
            font-size: 1rem;
        }
        """);
        return provider;
    }
}
