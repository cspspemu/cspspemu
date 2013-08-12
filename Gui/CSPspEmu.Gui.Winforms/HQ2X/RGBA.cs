using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HQ2x
{
	public struct BGRA
	{
		public byte B, G, R, A;

		static byte Clamp(int Value)
		{
			if (Value < 0) return 0;
			if (Value > 255) return 255;
			return (byte)Value;
		}

		internal static BGRA FromArgb(int R, int G, int B)
		{
			return FromArgb(255, R, G, B);
		}

		internal static BGRA FromArgb(int A, int R, int G, int B)
		{
			return new BGRA() { A = Clamp(A), R = Clamp(R), G = Clamp(G), B = Clamp(B) };
		}

		internal static BGRA FromColor(Color Color)
		{
			return new BGRA() { A = Color.A, R = Color.R, G = Color.G, B = Color.B };
		}

		public static bool operator !=(BGRA x, BGRA y)
		{
			return !(x == y);
		}

		public static bool operator ==(BGRA x, BGRA y)
		{
			return (x.R == y.R) && (x.G == y.G) && (x.B == y.B) && (x.A == y.A);
		}

		internal Color ToColor()
		{
			return Color.FromArgb(A, R, G, B);
		}
	}
}
