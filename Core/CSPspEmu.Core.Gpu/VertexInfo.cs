using System;
using System.Runtime.InteropServices;
using OpenTK;
using CSharpPlatform;

namespace CSPspEmu.Core.Gpu
{
	public static class Vector4fRawExtensions
	{
		public static Vector4fRaw Normalize(this Vector4fRaw Vector)
		{
			return Vector * (1.0f / (float)Math.Sqrt(Vector.X * Vector.X + Vector.Y * Vector.Y + Vector.Z * Vector.Z));
		}

		public static Vector3 ToVector3(this Vector4fRaw Vector)
		{
			return new Vector3(Vector.X, Vector.Y, Vector.Z);
		}
	}

	/// <summary>
	/// Information about a vertex.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct VertexInfo
	{
		public Vector4fRaw Color;
		public Vector4fRaw Position;
		public Vector4fRaw Normal;
		public Vector4fRaw Texture;
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
