using System.Drawing;
using CSharpUtils;

public static class ColorExtensions
{
    public static ushort Encode565(this Color Color)
    {
        return (ushort) Color.Encode(ColorFormats.RGBA_5650);
    }

    public static uint Encode(this Color Color, ColorFormat Format)
    {
        return Format.Encode(Color.R, Color.G, Color.B, Color.A);
    }
}