using System.Runtime.CompilerServices;

using Gdk;

using RiceTea.Core.Extensions;

namespace ShioExtend.GtkSharp.Utils;

public static partial class Keys
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsControlPressed() 
        => GetModifierState().HasFlagFast(ModifierType.ControlMask);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsShiftPressed() 
        => GetModifierState().HasFlagFast(ModifierType.ShiftMask);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAltPressed() 
        => GetModifierState().HasFlagFast(ModifierType.Mod1Mask);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumLockToggled() 
        => WindowMessageLoop.Invoke(IsNumLockToggledCore);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCapsLockToggled() 
        => WindowMessageLoop.Invoke(IsCapsLockToggledCore);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsScrollLockToggled() 
        => WindowMessageLoop.Invoke(IsScrollLockToggledCore);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ModifierType GetModifierState()
        => WindowMessageLoop.Invoke(GetModifierStateCore);
}
