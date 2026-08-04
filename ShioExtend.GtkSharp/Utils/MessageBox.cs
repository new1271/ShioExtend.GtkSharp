using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Gtk;

namespace ShioExtend.GtkSharp.Utils;

public static class MessageBox
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResponseType Show(Window owner, string text, string caption, 
        MessageType messageType = MessageType.Other, ButtonsType buttonType = ButtonsType.Ok)
        => WindowMessageLoop.Invoke(ShowCore, owner, (text, caption), (messageType, buttonType));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<ResponseType> ShowAsync(Window owner, string text, string caption, 
        MessageType messageType = MessageType.Other, ButtonsType buttonType = ButtonsType.Ok)
        => WindowMessageLoop.InvokeTaskAsync(ShowCore, owner, (text, caption), (messageType, buttonType));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ResponseType ShowCore(Window owner, (string text, string caption) tuple1, (MessageType messageType, ButtonsType buttonType) tuple2)
    {
        MessageDialog dialog = new MessageDialog(owner,
            DialogFlags.Modal | DialogFlags.DestroyWithParent | DialogFlags.UseHeaderBar,
            tuple2.messageType, tuple2.buttonType, null, null)
        {
            Title = tuple1.caption,
            Text = tuple1.text
        };
        try
        {
            return (ResponseType)dialog.Run();
        }
        finally
        {
            dialog.Destroy();
        }
    }
}