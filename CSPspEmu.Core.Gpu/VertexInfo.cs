using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Gpu
{
	public struct FVector3d
	{
		public float X, Y, Z;

		public FVector3d(float X, float Y, float Z)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	/// <summary>
	/// 20 floats.
	/// </summary>
	public struct VertexInfo
	{
		public float R, G, B, A;
		public float PX, PY, PZ;
		public float NX, NY, NZ;
		public float U, V;
		public float Weight0, Weight1, Weight2, Weight3, Weight4, Weight5, Weight6, Weight7;

		public FVector3d Position { get { return new FVector3d() { X = PX, Y = PY, Z = PZ }; } }
		public FVector3d Normal { get { return new FVector3d() { X = NX, Y = NY, Z = NZ }; } }

		public override string ToString()
		{
			return String.Format(
				"VertexInfo(Position=({0}, {1}, {2}), Normal=({3}, {4}, {5}), UV=({6}, {7}), COLOR=(R:{8}, G:{9}, B:{10}, A:{11}))",
				PX, PY, PZ,
				NX, NY, NZ,
				U, V,
				R, G, B, A
			);
		}
	}
}
