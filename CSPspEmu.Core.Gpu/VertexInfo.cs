using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Utils;
using OpenTK;

namespace CSPspEmu.Core.Gpu
{
	public struct Vector3F
	{
		public float X, Y, Z;

		public Vector3F(float X, float Y, float Z)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}

		static public implicit operator Vector3(Vector3F Vector3F)
		{
			return new Vector3(Vector3F.X, Vector3F.Y, Vector3F.Z);
		}

		static public implicit operator Vector3F(Vector3 Vector3)
		{
			return new Vector3F(Vector3.X, Vector3.Y, Vector3.Z);
		}

		static public Vector3F operator/(Vector3F Vector3F, float Value)
		{
			return new Vector3F(Vector3F.X / Value, Vector3F.Y / Value, Vector3F.Z / Value);
		}

		public override string ToString()
		{
			return String.Format("({0}, {1}, {2})", X, Y, Z);
		}
	}

	public struct Color4F
	{
		public float R, G, B, A;

		public Color4F(float R, float G, float B, float A)
		{
			this.R = R;
			this.G = G;
			this.B = B;
			this.A = A;
		}

		static public implicit operator OutputPixel(Color4F Color4F)
		{
			return new OutputPixel()
			{
				R = (byte)(Color4F.R * 0xFF),
				G = (byte)(Color4F.G * 0xFF),
				B = (byte)(Color4F.B * 0xFF),
				A = (byte)(Color4F.A * 0xFF),
			};
		}

		static public implicit operator Color4F(OutputPixel OutputPixel)
		{
			return new Color4F()
			{
				R = ((float)OutputPixel.R) / 255.0f,
				G = ((float)OutputPixel.G) / 255.0f,
				B = ((float)OutputPixel.B) / 255.0f,
				A = ((float)OutputPixel.A) / 255.0f,
			};
		}
	}

	/// <summary>
	/// Information about a vertex.
	/// </summary>
	unsafe public struct VertexInfo
	{
		public Color4F Color;
		public Vector3F Position;
		public Vector3F Normal;
		public Vector3F Texture;
		public fixed float Weights[8];

		public override string ToString()
		{
			return String.Format(
				"VertexInfo(Position={0}, Normal={1}, UV={2}, COLOR={3})",
				Position, Normal, Texture, Color
			);
		}
	}
}
