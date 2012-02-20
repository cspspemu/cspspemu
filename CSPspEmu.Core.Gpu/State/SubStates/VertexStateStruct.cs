using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Gpu.State
{
	public struct VertexTypeStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public bool ReversedNormal;
		
		/// <summary>
		/// 
		/// </summary>
		public byte NormalCount;

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

		public NumericEnum Weight {
			get { return (NumericEnum)BitUtils.Extract(Value, 9, 2); }
			set { BitUtils.Insert(ref Value, 9, 2, (uint)value); }
		}
		public NumericEnum Texture {
			get { return (NumericEnum)BitUtils.Extract(Value, 0, 2); }
			set { BitUtils.Insert(ref Value, 0, 2, (uint)value); }
		}
		public ColorEnum Color {
			get { return (ColorEnum)BitUtils.Extract(Value, 2, 3); }
			set { BitUtils.Insert(ref Value, 2, 3, (uint)value); }
		}
		public NumericEnum Normal {
			get { return (NumericEnum)BitUtils.Extract(Value, 5, 2); }
			set { BitUtils.Insert(ref Value, 5, 2, (uint)value); }
		}
		public NumericEnum Position {
			get { return (NumericEnum)BitUtils.Extract(Value, 7, 2); }
			set { BitUtils.Insert(ref Value, 7, 2, (uint)value); }
		}
		public IndexEnum Index {
			get { return (IndexEnum)BitUtils.Extract(Value, 11, 2); }
			set { BitUtils.Insert(ref Value, 11, 2, (uint)value); }
		}
		public uint SkinningWeightCount {
			get { return BitUtils.Extract(Value, 14, 3); }
			set { BitUtils.Insert(ref Value, 14, 3, (uint)value); }
		}
		public uint MorphingVertexCount {
			get { return BitUtils.Extract(Value, 18, 2); }
			set { BitUtils.Insert(ref Value, 18, 2, (uint)value); }
		}
		public bool Transform2D {
			get { return BitUtils.Extract(Value, 23, 1) != 0; }
			set { BitUtils.Insert(ref Value, 23, 1, value ? 1U : 0U); }
		}

		//public bool HasWeghts { get { return Weight != NumericEnum.Void; } }

		public uint SkinSize { get { return TypeSizeTable[(int)Weight]; } }
		public uint ColorSize { get { return ColorSizeTable[(int)Color]; } }
		public uint TextureSize { get { return TypeSizeTable[(int)Texture]; } }
		public uint PositionSize { get { return TypeSizeTable[(int)Position]; } }
		public uint NormalSize { get { return TypeSizeTable[(int)Normal]; } }

		//public uint StructAlignment { get { return Math.Max(Math.Max(Math.Max(Math.Max(SkinSize, ColorSize), TextureSize), PositionSize), NormalSize); } }
		public uint StructAlignment {
			get {
				//return Math.Max(Math.Max(Math.Max(Math.Max(SkinSize, ColorSize), TextureSize), PositionSize), NormalSize);
				return (uint)MathUtils.Max(SkinSize, ColorSize, TextureSize, PositionSize, NormalSize);
			}
		}

		public int GetMaxAlignment()
		{
			return (int)MathUtils.Max(SkinSize, ColorSize, TextureSize, PositionSize, NormalSize);
		}

		public uint GetVertexSize()
		{
			uint Size = 0;
			Size = (uint)MathUtils.NextAligned(Size, SkinSize); Size += SkinningWeightCount * SkinSize;
			Size = (uint)MathUtils.NextAligned(Size, TextureSize); Size += NormalCount * TextureSize;
			Size = (uint)MathUtils.NextAligned(Size, ColorSize); Size += 1 * ColorSize;
			Size = (uint)MathUtils.NextAligned(Size, NormalSize); Size += 3 * NormalSize;
			Size = (uint)MathUtils.NextAligned(Size, PositionSize); Size += 3 * PositionSize;

			var AlignmentSize = GetMaxAlignment();
			//Size = (uint)((Size + AlignmentSize - 1) & ~(AlignmentSize - 1));
			Size = (uint)MathUtils.NextAligned(Size, (uint)AlignmentSize);
			//Console.WriteLine("Size:" + Size);
			return Size;
		}

		/*
				vertexSize = 0;
				vertexSize += size_mapping[weight] * skinningWeightCount;
				vertexSize = (vertexSize + size_padding[texture]) & ~size_padding[texture];

				textureOffset = vertexSize;
				vertexSize += size_mapping[texture] * 2;
				vertexSize = (vertexSize + color_size_padding[color]) & ~color_size_padding[color];

				colorOffset = vertexSize;
				vertexSize += color_size_mapping[color];
				vertexSize = (vertexSize + size_padding[normal]) & ~size_padding[normal];

				normalOffset = vertexSize;
				vertexSize += size_mapping[normal] * 3;
				vertexSize = (vertexSize + size_padding[position]) & ~size_padding[position];

				positionOffset = vertexSize;
				vertexSize += size_mapping[position] * 3;

				oneVertexSize = vertexSize;
				vertexSize *= morphingVertexCount;

				alignmentSize = Math.max(size_mapping[weight],
						Math.max(color_size_mapping[color],
						Math.max(size_mapping[normal],
						Math.max(size_mapping[texture],
						size_mapping[position]))));
				vertexSize = (vertexSize + alignmentSize - 1) & ~(alignmentSize - 1);
				oneVertexSize = (oneVertexSize + alignmentSize - 1) & ~(alignmentSize - 1);
		 */

		public uint GetVertexSetMorphSize()
		{
			return GetVertexSize() * MorphingVertexCount;
		}

		public override string ToString()
		{
			return this.ToStringDefault();
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
