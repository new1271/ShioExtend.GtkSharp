using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Gtk;

using ShioExtend.GtkSharp.Windows;

namespace ShioExtend.GtkSharp.Utils;

public static class MessageBox
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DialogResult Show(NativeWindow owner, string text, string caption, MessageBoxFlags flags = MessageBoxFlags.Ok)
        => WindowMessageLoop.Invoke(ShowInternal, owner, (text, caption), flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DialogResult Show(Window owner, string text, string caption, MessageBoxFlags flags = MessageBoxFlags.Ok)
        => WindowMessageLoop.Invoke(ShowInternal, owner, (text, caption), flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<DialogResult> ShowAsync(NativeWindow owner, string text, string caption, MessageBoxFlags flags = MessageBoxFlags.Ok)
        => WindowMessageLoop.InvokeTaskAsync(ShowInternal, owner, (text, caption), flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<DialogResult> ShowAsync(Window owner, string text, string caption, MessageBoxFlags flags = MessageBoxFlags.Ok)
        => WindowMessageLoop.InvokeTaskAsync(ShowInternal, owner, (text, caption), flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DialogResult ShowInternal(NativeWindow owner, (string text, string caption) tuple, MessageBoxFlags flags)
        => ShowInternal(owner.Window!, tuple, flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DialogResult ShowInternal(Window owner, (string text, string caption) tuple, MessageBoxFlags flags)
    {
        int retVal = ShowCore(owner, tuple.text, tuple.caption, flags);
        if (retVal >= 0)
            return (DialogResult)retVal;
        return retVal switch
        {
            (int)ResponseType.DeleteEvent or
            (int)ResponseType.Close or
            (int)ResponseType.Cancel => GetButtonFlags(flags) switch
            {
                MessageBoxFlags.Ok => DialogResult.Ok,
                MessageBoxFlags.YesNo or MessageBoxFlags.AbortRetryIgnore => DialogResult.Invalid, // Won't happen in regular situation
                _ => DialogResult.Cancel
            },
            _ => DialogResult.Invalid
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ShowCore(Window owner, string text, string caption, MessageBoxFlags flags)
    {
        MessageDialog dialog = new MessageDialog(owner,
                    DialogFlags.Modal | DialogFlags.DestroyWithParent | DialogFlags.UseHeaderBar,
                    MessageType.Other, ButtonsType.None, null, null)
        {
            Title = caption,
            Text = text
        };

        switch (GetButtonFlags(flags))
        {
            case MessageBoxFlags.Ok:
                dialog.AddButton(Stock.Ok, (int)DialogResult.Ok);
                break;
            case MessageBoxFlags.OkCancel:
                dialog.AddButton(Stock.Cancel, (int)DialogResult.Cancel);
                dialog.AddButton(Stock.Ok, (int)DialogResult.Ok);
                dialog.DefaultResponse = (ResponseType)(GetDefaultButtonIndex(flags) switch
                {
                    1 => DialogResult.Cancel,
                    _ => DialogResult.Ok,
                });
                break;
            case MessageBoxFlags.AbortRetryIgnore:
                dialog.AddButton(Stock.Stop, (int)DialogResult.Abort);
                dialog.AddButton(Stock.Discard, (int)DialogResult.Ignore);
                dialog.AddButton(Stock.Refresh, (int)DialogResult.Retry);
                dialog.DefaultResponse = (ResponseType)(GetDefaultButtonIndex(flags) switch
                {
                    1 => DialogResult.Retry,
                    2 => DialogResult.Ignore,
                    _ => DialogResult.Abort,
                });
                dialog.Deletable = false;
                break;
            case MessageBoxFlags.YesNoCancel:
                dialog.AddButton(Stock.Cancel, (int)DialogResult.Cancel);
                dialog.AddButton(Stock.No, (int)DialogResult.No);
                dialog.AddButton(Stock.Yes, (int)DialogResult.Yes);
                dialog.DefaultResponse = (ResponseType)(GetDefaultButtonIndex(flags) switch
                {
                    1 => DialogResult.No,
                    2 => DialogResult.Cancel,
                    _ => DialogResult.Yes,
                });
                break;
            case MessageBoxFlags.YesNo:
                dialog.AddButton(Stock.No, (int)DialogResult.No);
                dialog.AddButton(Stock.Yes, (int)DialogResult.Yes);
                dialog.DefaultResponse = (ResponseType)(GetDefaultButtonIndex(flags) switch
                {
                    1 => DialogResult.No,
                    _ => DialogResult.Yes,
                });
                dialog.Deletable = false;
                break;
            case MessageBoxFlags.RetryCancel:
                dialog.AddButton(Stock.Cancel, (int)DialogResult.Cancel);
                dialog.AddButton(Stock.Refresh, (int)DialogResult.Retry);
                dialog.DefaultResponse = (ResponseType)(GetDefaultButtonIndex(flags) switch
                {
                    1 => DialogResult.Cancel,
                    _ => DialogResult.Retry,
                });
                break;
            case MessageBoxFlags.CancelRetryContinue:
                dialog.AddButton(Stock.Cancel, (int)DialogResult.Cancel);
                dialog.AddButton(Stock.Refresh, (int)DialogResult.Retry);
                dialog.AddButton(Stock.GoForward, (int)DialogResult.Continue);
                dialog.DefaultResponse = (ResponseType)(GetDefaultButtonIndex(flags) switch
                {
                    1 => DialogResult.Retry,
                    2 => DialogResult.Continue,
                    _ => DialogResult.Cancel,
                });
                break;
            default:
                return (int)DialogResult.Invalid;
        }

        dialog.MessageType = GetIconFlags(flags) switch
        {
            MessageBoxFlags.IconQuestion => MessageType.Question,
            MessageBoxFlags.IconInformation => MessageType.Info,
            MessageBoxFlags.IconWarning => MessageType.Warning,
            MessageBoxFlags.IconError => MessageType.Error,
            _ => MessageType.Other
        };

        try
        {
            return dialog.Run();
        }
        finally
        {
            dialog.Destroy();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetDefaultButtonIndex(MessageBoxFlags flags) => (byte)(((uint)flags & 0x00000F00U) >> 8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MessageBoxFlags GetButtonFlags(MessageBoxFlags flags) => (MessageBoxFlags)((uint)flags & 0x0000000FU);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MessageBoxFlags GetIconFlags(MessageBoxFlags flags) => (MessageBoxFlags)((uint)flags & 0x000000F0U);
}

[Flags]
public enum MessageBoxFlags : uint
{
    Ok = 0x00000000U,
    OkCancel = 0x00000001U,
    AbortRetryIgnore = 0x00000002U,
    YesNoCancel = 0x00000003U,
    YesNo = 0x00000004U,
    RetryCancel = 0x00000005U,
    CancelRetryContinue = 0x00000006U,

    IconHand = 0x00000010U,
    IconQuestion = 0x00000020U,
    IconExclamation = 0x00000030U,
    IconAsterisk = 0x00000040U,

    // UserIcon = 0x00000080U,
    IconWarning = IconExclamation,
    IconError = IconHand,

    IconInformation = IconAsterisk,
    IconStop = IconHand,

    DefaultButton1 = 0x00000000U,
    DefaultButton2 = 0x00000100U,
    DefaultButton3 = 0x00000200U,
    DefaultButton4 = 0x00000300U
}