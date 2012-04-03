using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Utils
{
	public struct OutputPixel
	{
		public byte R, G, B, A;
		//public byte B, G, R, A;

		public OutputPixel(Color Color)
		{
			R = Color.R;
			G = Color.G;
			B = Color.B;
			A = Color.A;
		}

		public override string ToString()
		{
			return String.Format("RGBA({0},{1},{2},{3})", R, G, B, A);
		}

		static public OutputPixel OperationPerComponent(OutputPixel c1, OutputPixel c2, Func<byte, byte, byte> func)
		{
			return new OutputPixel()
			{
				R = func(c1.R, c2.R),
				G = func(c1.G, c2.G),
				B = func(c1.B, c2.B),
				A = func(c1.A, c2.A),
			};
		}

		public static OutputPixel operator &(OutputPixel c1, OutputPixel c2)
		{
			return new OutputPixel()
			{
				R = (byte)(c1.R & c2.R),
				G = (byte)(c1.G & c2.G),
				B = (byte)(c1.B & c2.B),
				A = (byte)(c1.A & c2.A),
			};
		}

		public static bool operator ==(OutputPixel c1, OutputPixel c2)
		{
			return (
				(c1.R == c2.R) &&
				(c1.G == c2.G) &&
				(c1.B == c2.B) &&
				(c1.A == c2.A)
			);
		}

		public static bool operator !=(OutputPixel c1, OutputPixel c2)
		{
			return !(c1 == c2);
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() != typeof(OutputPixel)) return false;
			return (OutputPixel)obj == this;
		}

		public override int GetHashCode()
		{
			return (R << 0) | (G << 8) | (B << 16) | (A << 24);
		}

		public int CheckSum { get { return (int)R + (int)G + (int)B + (int)A; } }
	}
}
