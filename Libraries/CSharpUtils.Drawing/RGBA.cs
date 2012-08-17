using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpUtils.Drawing
{
	/// <summary>
	/// 
	/// </summary>
	public struct RGBA
	{
		/// <summary>
		/// 
		/// </summary>
		public byte R, G, B, A;

		public RGBA(byte R, byte G, byte B, byte A)
		{
			this.A = A;
			this.R = R;
			this.G = G;
			this.B = B;
		}

		public static implicit operator RGBA(ARGB_Rev Col)
		{
			return new RGBA(Col.R, Col.G, Col.B, Col.A);
		}

		public static implicit operator ARGB_Rev(RGBA Col)
		{
			return new ARGB_Rev(Col.A, Col.R, Col.G, Col.B);
		}
	}

	public struct RGBA32
	{
		public int R, G, B, A;

		public RGBA32(int R, int G, int B, int A)
		{
			this.A = A;
			this.R = R;
			this.G = G;
			this.B = B;
		}

		static public RGBA32 operator +(RGBA32 Accum, ARGB_Rev In)
		{
			return new RGBA32()
			{
				R = Accum.R + In.R,
				G = Accum.G + In.G,
				B = Accum.B + In.B,
				A = Accum.A + In.A,
			};
		}

		static public RGBA32 operator *(RGBA32 Accum, int Value)
		{
			return new RGBA32()
			{
				R = Accum.R * Value,
				G = Accum.G * Value,
				B = Accum.B * Value,
				A = Accum.A * Value,
			};
		}

		static public RGBA32 operator /(RGBA32 Accum, int Value)
		{
			if (Value == 0)
			{
				return new RGBA32(0, 0, 0, 0);
			}
			else
			{
				return new RGBA32()
				{
					R = Accum.R / Value,
					G = Accum.G / Value,
					B = Accum.B / Value,
					A = Accum.A / Value,
				};
			}
		}

		public static implicit operator RGBA32(ARGB_Rev Col)
		{
			return new RGBA32() { R = Col.R, G = Col.G, B = Col.B, A = Col.A };
		}

		public static implicit operator ARGB_Rev(RGBA32 Col)
		{
			var A = (byte)MathUtils.FastClamp(Col.A, 0x00, 0xFF);
			var R = (byte)MathUtils.FastClamp(Col.R, 0x00, 0xFF);
			var G = (byte)MathUtils.FastClamp(Col.G, 0x00, 0xFF);
			var B = (byte)MathUtils.FastClamp(Col.B, 0x00, 0xFF);
			return new ARGB_Rev(A, R, G, B);
		}
	}

	public struct RGBAFloat
	{
		public float R, G, B, A;
	}

	/// <summary>
	/// 
	/// </summary>
	public struct ARGB_Rev
	{
		/// <summary>
		/// 
		/// </summary>
		public byte B, G, R, A;

		public ARGB_Rev(byte A, byte R, byte G, byte B)
		{
			this.A = A;
			this.R = R;
			this.G = G;
			this.B = B;
		}

		static public int DistanceRGB(ARGB_Rev Color1, ARGB_Rev Color2)
		{
			var R = Math.Abs(Color1.R - Color2.R);
			var G = Math.Abs(Color1.G - Color2.G);
			var B = Math.Abs(Color1.B - Color2.B);
			return (R * R) + (G * G) + (B * B);
		}

		public static implicit operator ARGB_Rev(string Col)
		{
			if (Col.StartsWith("#"))
			{
				Col = Col.Substring(1);

				byte R, G, B, A = 0xFF;

				if (Col.Length >= 6)
				{
					R = (byte)((Convert.ToInt32(Col.Substr(0, 2), 16) * 255) / 255);
					G = (byte)((Convert.ToInt32(Col.Substr(2, 2), 16) * 255) / 255);
					B = (byte)((Convert.ToInt32(Col.Substr(4, 2), 16) * 255) / 255);
					if (Col.Length >= 8)
					{
						A = (byte)((Convert.ToInt32(Col.Substr(6, 2), 16) * 255) / 255);
					}
				}
				else if (Col.Length >= 3)
				{
					R = (byte)((Convert.ToInt32(Col.Substr(0, 1), 16) * 255) / 15);
					G = (byte)((Convert.ToInt32(Col.Substr(1, 1), 16) * 255) / 15);
					B = (byte)((Convert.ToInt32(Col.Substr(2, 1), 16) * 255) / 15);
					if (Col.Length >= 4)
					{
						A = (byte)((Convert.ToInt32(Col.Substr(3, 1), 16) * 255) / 15);
					}
				}
				else
				{
					throw(new NotImplementedException());
				}

				return new ARGB_Rev(A, R, G, B);
			}
			throw(new NotImplementedException());
		}

		public static implicit operator ARGB_Rev(Color Col)
		{
			return new ARGB_Rev() { R = Col.R, G = Col.G, B = Col.B, A = Col.A };
		}

		public static implicit operator Color(ARGB_Rev Col)
		{
			return Color.FromArgb(Col.A, Col.R, Col.G, Col.B);
		}

		public override string ToString()
		{
			return String.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", R, G, B, A);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public struct ARGB
	{
		/// <summary>
		/// 
		/// </summary>
		public byte A, R, G, B;
	}
}
