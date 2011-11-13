using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Gpu.State
{
	public struct VertexTypeStruct
	{
		readonly static public uint[] TypeSize = new uint[] { 0, sizeof(byte), sizeof(short), sizeof(float) };
		readonly static public uint[] ColorSize = new uint[] { 0, 1, 1, 1, 2, 2, 2, 4 };

		public uint Value;

		public uint Texture { get { return BitUtils.Extract(Value, 0, 2); } }
		public uint Color   { get { return BitUtils.Extract(Value, 2, 3); } }
		public uint Normal  { get { return BitUtils.Extract(Value, 5, 2); } }
		public uint Position { get { return BitUtils.Extract(Value, 7, 2); } }
		public uint Weight { get { return BitUtils.Extract(Value, 9, 2); } }
		public uint Index { get { return BitUtils.Extract(Value, 11, 2); } }
		public uint SkinningWeightCount { get { return BitUtils.Extract(Value, 14, 3); } }
		public uint MorphingVertexCount { get { return BitUtils.Extract(Value, 18, 2); } }
		public bool Transform2D { get { return BitUtils.Extract(Value, 23, 1) != 0; } }

		public uint GetVertexSize()
		{
			uint Size = 0;
			Size += SkinningWeightCount * TypeSize[(int)Weight];
			Size += 1 * ColorSize[(int)Color];
			Size += 2 * TypeSize[(int)Texture];
			Size += 3 * TypeSize[(int)Position];
			Size += 3 * TypeSize[(int)Normal];
			return Size;
		}

		public uint GetVertexSetMorphSize()
		{
			return GetVertexSize() * MorphingVertexCount;
		}
	}

	public enum TransformModeEnum
	{
		Normal = 0,
		Raw = 1,
	}

	public struct VertexStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public GpuMatrix4x4Struct ProjectionMatrix;
		
		/// <summary>
		/// 
		/// </summary>
		public GpuMatrix4x3Struct WorldMatrix;
		
		/// <summary>
		/// 
		/// </summary>
		public GpuMatrix4x3Struct ViewMatrix;

		/// <summary>
		/// 
		/// </summary>
		public TransformModeEnum TransformMode;

		/// <summary>
		/// here because of transform2d
		/// </summary>
		public VertexTypeStruct Type;
	}
}
