using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using CSharpUtils;

static public class ColorExtensions
{
	static public ushort Encode565(this Color Color)
	{
		return (ushort)Color.Encode(ColorFormats.RGBA_5650);
	}

	static public uint Encode(this Color Color, ColorFormat Format)
	{
		return Format.Encode(Color.R, Color.G, Color.B, Color.A);
	}
}
