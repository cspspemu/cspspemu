using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using CSharpUtils.Drawing;

namespace CSharpUtils
{
	public class ColorUtils
	{
		[Obsolete("Use RGBA32 + operator")]
		static public void InternalAdd(ref int R, ref int G, ref int B, ref int A, params Color[] Colors)
		{
			foreach (var Color in Colors)
			{
				R += Color.R;
				G += Color.G;
				B += Color.B;
				A += Color.A;
			}
		}

		static public Color Average(params Color[] Colors)
		{
			int R = 0, G = 0, B = 0, A = 0;
			int L = Colors.Length;
			InternalAdd(ref R, ref G, ref B, ref A, Colors);
			if (L == 0) L = 1;
			return Color.FromArgb(
				(byte)(A / L),
				(byte)(R / L),
				(byte)(G / L),
				(byte)(B / L)
			);
		}

		static public Color Average(Color Color1, Color Color2)
		{
			return Color.FromArgb(
				(byte)((Color1.A + Color2.A) / 2),
				(byte)((Color1.R + Color2.R) / 2),
				(byte)((Color1.G + Color2.G) / 2),
				(byte)((Color1.B + Color2.B) / 2)
			);
		}

		static public Color Add(params Color[] Colors)
		{
			int R = 0, G = 0, B = 0, A = 0;
			InternalAdd(ref R, ref G, ref B, ref A, Colors);
			return Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B);
		}

		static public Color Add(Color Color1, Color Color2)
		{
			return Color.FromArgb(
				(byte)((Color1.A + Color2.A)),
				(byte)((Color1.R + Color2.R)),
				(byte)((Color1.G + Color2.G)),
				(byte)((Color1.B + Color2.B))
			);
		}

		static public Color Substract(Color ColorLeft, Color ColorRight)
		{
			return Color.FromArgb(
				(byte)(ColorLeft.A - ColorRight.A),
				(byte)(ColorLeft.R - ColorRight.R),
				(byte)(ColorLeft.G - ColorRight.G),
				(byte)(ColorLeft.B - ColorRight.B)
			);
		}

		static public Color Average(Bitmap Bitmap)
		{
			Color[] Colors = new Color[Bitmap.Width * Bitmap.Height];
			for (int y = 0, n = 0; y < Bitmap.Height; y++)
			{
				for (int x = 0; x < Bitmap.Width; x++, n++)
				{
					Colors[n] = Bitmap.GetPixel(x, y);
				}
			}
			return Average(Colors);
		}

		public static Color Encode(ColorFormat ColorFormat, uint Value)
		{
			return Color.FromArgb(
				(int)(BitUtils.ExtractScaled(Value, ColorFormat.Alpha.Offset, ColorFormat.Alpha.Size, 255)),
				(int)(BitUtils.ExtractScaled(Value, ColorFormat.Red.Offset, ColorFormat.Red.Size, 255)),
				(int)(BitUtils.ExtractScaled(Value, ColorFormat.Green.Offset, ColorFormat.Green.Size, 255)),
				(int)(BitUtils.ExtractScaled(Value, ColorFormat.Blue.Offset, ColorFormat.Blue.Size, 255))
			);
		}

		public static int MixComponent(int Color1, int Color2, int WeightSum, int Weight1, int Weight2)
		{
			return (Color1 * Weight1 + Color2 * Weight2) / WeightSum;
		}

		public static Color Mix(Color Color1, Color Color2, int WeightSum, int Weight1, int Weight2)
		{
			return Color.FromArgb(
				MixComponent(Color1.A, Color2.A, WeightSum, Weight1, Weight2),
				MixComponent(Color1.R, Color2.R, WeightSum, Weight1, Weight2),
				MixComponent(Color1.G, Color2.G, WeightSum, Weight1, Weight2),
				MixComponent(Color1.B, Color2.B, WeightSum, Weight1, Weight2)
			);
		}
	}
}
