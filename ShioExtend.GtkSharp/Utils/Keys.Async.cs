using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Gdk;

using RiceTea.Core.Extensions;

namespace ShioExtend.GtkSharp.Utils;

partial class Keys
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsControlPressedAsync()
        => GetModifierStateAsync().ContinueWith(static task => task.Result.HasFlagFast(ModifierType.ControlMask));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsShiftPressedAsync()
        => GetModifierStateAsync().ContinueWith(static task => task.Result.HasFlagFast(ModifierType.ShiftMask));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsAltPressedAsync()
        => GetModifierStateAsync().ContinueWith(static task => task.Result.HasFlagFast(ModifierType.Mod1Mask));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsNumLockToggledAsync()
        => WindowMessageLoop.InvokeTaskAsync(IsNumLockToggledCore);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsCapsLockToggledAsync()
        => WindowMessageLoop.InvokeTaskAsync(IsCapsLockToggledCore);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsScrollLockToggledAsync()
        => WindowMessageLoop.InvokeTaskAsync(IsScrollLockToggledCore);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ModifierType> GetModifierStateAsync()
        => WindowMessageLoop.InvokeTaskAsync(GetModifierStateCore);
}
