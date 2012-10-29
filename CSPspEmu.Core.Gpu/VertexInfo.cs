using System;
using System.Runtime.InteropServices;
using OpenTK;
using Mono.Simd;

namespace CSPspEmu.Core.Gpu
{
#if false
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Vector3F
	{
		public float X, Y, Z;

		public static Vector3F Zero = new Vector3F(0, 0, 0);

		public Vector3F(float X, float Y, float Z)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}

		public static implicit operator Vector3(Vector3F Vector3F)
		{
			return new Vector3(Vector3F.X, Vector3F.Y, Vector3F.Z);
		}

		public static implicit operator Vector3F(Vector3 Vector3)
		{
			return new Vector3F(Vector3.X, Vector3.Y, Vector3.Z);
		}

		public static Vector3F operator +(Vector3F Left, Vector3F Right)
		{
			return new Vector3F(Left.X + Right.X, Left.Y + Right.Y, Left.Z + Right.Z);
		}

		public static Vector3F operator/(Vector3F Left, float Value)
		{
			return new Vector3F(Left.X / Value, Left.Y / Value, Left.Z / Value);
		}

		public static Vector3F operator*(Vector3F Left, float Value)
		{
			return new Vector3F(Left.X * Value, Left.Y * Value, Left.Z * Value);
		}

		public override string ToString()
		{
			return String.Format("({0}, {1}, {2})", X, Y, Z);
		}

		public Vector3F Normalize()
		{
			return this / (float)Math.Sqrt(X * X + Y * Y + Z * Z);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
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

		public static implicit operator OutputPixel(Color4F Color4F)
		{
			return new OutputPixel()
			{
				R = (byte)(Color4F.R * 0xFF),
				G = (byte)(Color4F.G * 0xFF),
				B = (byte)(Color4F.B * 0xFF),
				A = (byte)(Color4F.A * 0xFF),
			};
		}

		public static implicit operator Color4F(OutputPixel OutputPixel)
		{
			return new Color4F()
			{
				R = ((float)OutputPixel.R) / 255.0f,
				G = ((float)OutputPixel.G) / 255.0f,
				B = ((float)OutputPixel.B) / 255.0f,
				A = ((float)OutputPixel.A) / 255.0f,
			};
		}

		public static Color4F operator +(Color4F Left, Color4F Right)
		{
			return new Color4F(
				Left.R + Right.R,
				Left.G + Right.G,
				Left.B + Right.B,
				Left.A + Right.A
			);
		}

		public static Color4F operator *(Color4F Left, float Value)
		{
			return new Color4F(
				Left.R + Value,
				Left.G + Value,
				Left.B + Value,
				Left.A + Value
			);
		}

		public override string ToString()
		{
			return String.Format("Color4F({0}, {1}, {2}, {3})", R, G, B, A);
		}
	}
#endif

	public static class Vector4fExtensions
	{
		public static Vector4f Normalize(this Vector4f Vector)
		{
			return Vector * (1.0f / (float)Math.Sqrt(Vector.X * Vector.X + Vector.Y * Vector.Y + Vector.Z * Vector.Z));
		}

		public static Vector3 ToVector3(this Vector4f Vector)
		{
			return new Vector3(Vector.X, Vector.Y, Vector.Z);
		}
	}


	/// <summary>
	/// Information about a vertex.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public unsafe struct VertexInfo
	{
		public Vector4f Color;
		public Vector4f Position;
		public Vector4f Normal;
		public Vector4f Texture;
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
