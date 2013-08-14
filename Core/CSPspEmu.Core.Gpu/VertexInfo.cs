using System;
using System.Runtime.InteropServices;
//using OpenTK;
using CSharpPlatform;
using CSharpPlatform.GL.Utils;

namespace CSPspEmu.Core.Gpu
{
	public static class Vector4fRawExtensions
	{
		public static Vector4f Normalize(this Vector4f Vector)
		{
			return Vector * (1.0f / (float)Math.Sqrt(Vector.X * Vector.X + Vector.Y * Vector.Y + Vector.Z * Vector.Z));
		}

		public static Vector4f ToVector3(this Vector4f Vector)
		{
			return new Vector4f(Vector.X, Vector.Y, Vector.Z, 0f);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct VertexInfoVector3f
	{
		public float X, Y, Z;

		public VertexInfoVector3f(Vector4f Vector4f)
		{
			this.X = Vector4f.X;
			this.Y = Vector4f.Y;
			this.Z = Vector4f.Z;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct VertexInfoColor
	{
		public float R, G, B, A;

		public VertexInfoColor(Vector4f Vector4f)
		{
			this.R = Vector4f.X;
			this.G = Vector4f.Y;
			this.B = Vector4f.Z;
			this.A = Vector4f.W;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct VertexInfoWeights
	{
		public fixed float W[8];

		public unsafe VertexInfoWeights(VertexInfo VertexInfo)
		{
			fixed (float* WPtr = W)
			{
				for (int n = 0; n < 8; n++) WPtr[n] = VertexInfo.Weights[n];
			}
		}
	}

	/// <summary>
	/// Information about a vertex.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
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
