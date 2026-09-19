using System.Runtime.InteropServices;

namespace ShioExtend.GtkSharp.Windows;

[StructLayout(LayoutKind.Auto)]
public struct CreateWindowInfo
{
    public const int UseDefaultSizeOrLocation = unchecked((int)0x80000000);

    public int X;
    public int Y;
    public int Width;
    public int Height;

    public CreateWindowInfo() : this(
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation)
    { }

    public CreateWindowInfo(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
