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
