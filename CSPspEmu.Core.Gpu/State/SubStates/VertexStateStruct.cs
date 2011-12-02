using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Gpu.State
{
	public struct VertexTypeStruct
	{
		readonly static public uint[] TypeSizeTable = new uint[] { 0, sizeof(byte), sizeof(short), sizeof(float) };
		readonly static public uint[] ColorSizeTable = new uint[] { 0, 1, 1, 1, 2, 2, 2, 4 };

		public enum IndexEnum
		{
			Void = 0,
			Byte = 1,
			Short = 2,
			Invalid1 = 1,
		}

		public enum NumericEnum
		{
			Void = 0,
			Byte = 1,
			Short = 2,
			Float = 3,
		}

		public enum ColorEnum
		{
			Void = 0,
			Invalid1 = 1,
			Invalid2 = 2,
			Invalid3 = 3,
			Color5650 = 4,
			Color5551 = 5,
			Color4444 = 6,
			Color8888 = 7,
		}

		public uint Value;

		public NumericEnum Texture { get { return (NumericEnum)BitUtils.Extract(Value, 0, 2); } }
		public ColorEnum Color { get { return (ColorEnum)BitUtils.Extract(Value, 2, 3); } }
		public NumericEnum Normal { get { return (NumericEnum)BitUtils.Extract(Value, 5, 2); } }
		public NumericEnum Position { get { return (NumericEnum)BitUtils.Extract(Value, 7, 2); } }
		public NumericEnum Weight { get { return (NumericEnum)BitUtils.Extract(Value, 9, 2); } }
		public IndexEnum Index { get { return (IndexEnum)BitUtils.Extract(Value, 11, 2); } }
		public uint SkinningWeightCount { get { return BitUtils.Extract(Value, 14, 3); } }
		public uint MorphingVertexCount { get { return BitUtils.Extract(Value, 18, 2); } }
		public bool Transform2D { get { return BitUtils.Extract(Value, 23, 1) != 0; } }

		public uint GetVertexSize()
		{
			uint Size = 0;
			var SkinSize = TypeSizeTable[(int)Weight];
			var ColorSize = ColorSizeTable[(int)Color];
			var TextureSize = TypeSizeTable[(int)Texture];
			var PositionSize = TypeSizeTable[(int)Position];
			var NormalSize = TypeSizeTable[(int)Normal];
			Size += SkinningWeightCount * SkinSize;
			Size += 1 * ColorSize;
			Size += 2 * TextureSize;
			Size += 3 * PositionSize;
			Size += 3 * NormalSize;
			Size = MathUtils.NextAligned(
				Size,
				(int)(new uint[] { SkinSize, ColorSize, TextureSize, PositionSize, NormalSize }).Max()
			);
			//Console.WriteLine("Size:" + Size);
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
