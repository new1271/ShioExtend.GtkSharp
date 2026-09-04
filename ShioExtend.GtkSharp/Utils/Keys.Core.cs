using System.Runtime.CompilerServices;

using Gdk;

namespace ShioExtend.GtkSharp.Utils;

partial class Keys
{
    private static readonly Keymap? _keymap = GetDefaultKeymap();

    private static Keymap? GetDefaultKeymap()
    {
        Display? display = Display.Default;
        return display is null ? null : Keymap.GetForDisplay(display);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ModifierType GetModifierStateCore() => (ModifierType)(_keymap?.ModifierState ?? 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNumLockToggledCore() => _keymap?.NumLockState ?? false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCapsLockToggledCore() => _keymap?.CapsLockState ?? false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsScrollLockToggledCore() => _keymap?.ScrollLockState ?? false;
}
