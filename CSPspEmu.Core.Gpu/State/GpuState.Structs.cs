using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class GpuState
	{
		public GpuStateStruct Value;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PointI
	{
		public uint X, Y;

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PointS
	{
		public short X, Y;

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ViewportStruct
	{
		public Vector3F Position;
		public Vector3F Scale;
		public PointS RegionTopLeft;
		public PointS RegionBottomRight;

		public PointS RegionSize
		{
			get
			{
				return new PointS()
				{
					X = (short)(RegionBottomRight.X - RegionTopLeft.X + 1),
					Y = (short)(RegionBottomRight.Y - RegionTopLeft.Y + 1),
				};
			}
		}

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ColorStruct
	{
		public byte iRed, iGreen, iBlue, iAlpha;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ColorfStruct
	{
		public float Red, Green, Blue, Alpha;

		public void SetRGB_A1(uint Params24)
		{
			SetRGB(Params24);
			Alpha = 1.0f;
		}

		public void SetRGB(uint Params24)
		{
			//Console.WriteLine(Params24);
			Red = ((float)((Params24 >> 0) & 0xFF)) / 255.0f;
			Green = ((float)((Params24 >> 8) & 0xFF)) / 255.0f;
			Blue = ((float)((Params24 >> 16) & 0xFF)) / 255.0f;
		}

		public void SetA(uint Params24)
		{
			Alpha = ((float)((Params24 >> 0) & 0xFF)) / 255.0f;
		}

		public override string ToString()
		{
			return String.Format("Colorf(R={0}, G={1}, B={2}, A={3})", Red, Green, Blue, Alpha);
		}

		public bool IsColorf(float R, float G, float B)
		{
			return R == this.Red && G == this.Green && B == this.Blue;
		}

		public static ColorfStruct operator +(ColorfStruct Left, ColorfStruct Right)
		{
			return new ColorfStruct()
			{
				Red = Left.Red + Right.Red,
				Green = Left.Green + Right.Green,
				Blue = Left.Blue + Right.Blue,
				Alpha = Left.Alpha + Right.Alpha,
			};
		}

	}
}
