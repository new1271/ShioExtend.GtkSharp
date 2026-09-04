using System.Runtime.CompilerServices;

using Gdk;

using Gtk;

using ShioExtend.GtkSharp.Internals;

namespace ShioExtend.GtkSharp;

public static class ShioExtendGtkSharp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyFallbackStyles()
    {
        WindowMessageLoop.ThrowIfNotInMessageLoopThread();

        StyleContext.AddProviderForScreen(Screen.Default, StyleProviders.ScreenWideFallbackStyleProvider, StyleProviderPriority.Fallback);
    }
}
