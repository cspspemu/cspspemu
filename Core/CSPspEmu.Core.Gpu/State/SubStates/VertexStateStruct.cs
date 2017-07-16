using System.Runtime.InteropServices;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Gpu.State
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
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

        /// <summary>
        /// 
        /// </summary>
        public uint Value;

        static public bool operator ==(VertexTypeStruct a, VertexTypeStruct b)
        {
            return (a.ReversedNormal == b.ReversedNormal) && (a.NormalCount == b.NormalCount) && (a.Value == b.Value);
        }

        static public bool operator !=(VertexTypeStruct a, VertexTypeStruct b)
        {
            return !(a == b);
        }

        public static readonly int[] TypeSizeTable = new int[] {0, sizeof(byte), sizeof(short), sizeof(float)};
        public static readonly int[] ColorSizeTable = new int[] {0, 1, 1, 1, 2, 2, 2, 4};

        public enum IndexEnum : byte
        {
            Void = 0,
            Byte = 1,
            Short = 2,
            //Invalid1 = 1,
        }

        public enum NumericEnum : byte
        {
            Void = 0,
            Byte = 1,
            Short = 2,
            Float = 3,
        }

        public enum ColorEnum : byte
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

        public bool HasWeight
        {
            get { return Weight != NumericEnum.Void; }
        }

        public NumericEnum Weight
        {
            get { return (NumericEnum) BitUtils.Extract(Value, 9, 2); }
            set { BitUtils.Insert(ref Value, 9, 2, (uint) value); }
        }

        public bool HasTexture
        {
            get { return Texture != VertexTypeStruct.NumericEnum.Void; }
        }

        public NumericEnum Texture
        {
            get { return (NumericEnum) BitUtils.Extract(Value, 0, 2); }
            set { BitUtils.Insert(ref Value, 0, 2, (uint) value); }
        }

        public bool HasColor
        {
            get { return Color != VertexTypeStruct.ColorEnum.Void; }
        }

        public ColorEnum Color
        {
            get { return (ColorEnum) BitUtils.Extract(Value, 2, 3); }
            set { BitUtils.Insert(ref Value, 2, 3, (uint) value); }
        }

        public bool HasNormal
        {
            get { return Normal != VertexTypeStruct.NumericEnum.Void; }
        }

        public NumericEnum Normal
        {
            get { return (NumericEnum) BitUtils.Extract(Value, 5, 2); }
            set { BitUtils.Insert(ref Value, 5, 2, (uint) value); }
        }

        public bool HasPosition
        {
            get { return Position != VertexTypeStruct.NumericEnum.Void; }
        }

        public NumericEnum Position
        {
            get { return (NumericEnum) BitUtils.Extract(Value, 7, 2); }
            set { BitUtils.Insert(ref Value, 7, 2, (uint) value); }
        }

        public bool HasIndex
        {
            get { return Index != VertexTypeStruct.IndexEnum.Void; }
        }

        public IndexEnum Index
        {
            get { return (IndexEnum) BitUtils.Extract(Value, 11, 2); }
            set { BitUtils.Insert(ref Value, 11, 2, (uint) value); }
        }

        public int SkinningWeightCount
        {
            get { return (int) BitUtils.Extract(Value, 14, 3); }
            set { BitUtils.Insert(ref Value, 14, 3, (uint) value); }
        }

        public int MorphingVertexCount
        {
            get { return (int) BitUtils.Extract(Value, 18, 2); }
            set { BitUtils.Insert(ref Value, 18, 2, (uint) value); }
        }

        public bool Transform2D
        {
            get { return BitUtils.Extract(Value, 23, 1) != 0; }
            set { BitUtils.Insert(ref Value, 23, 1, value ? 1U : 0U); }
        }

        public int RealSkinningWeightCount
        {
            get
            {
                if (SkinSize == 0) return 0;
                return SkinningWeightCount + 1;
            }
        }

        //public bool HasWeghts { get { return Weight != NumericEnum.Void; } }

        public int SkinSize
        {
            get { return TypeSizeTable[(int) Weight]; }
        }

        public int ColorSize
        {
            get { return ColorSizeTable[(int) Color]; }
        }

        public int TextureSize
        {
            get { return TypeSizeTable[(int) Texture]; }
        }

        public int PositionSize
        {
            get { return TypeSizeTable[(int) Position]; }
        }

        public int NormalSize
        {
            get { return TypeSizeTable[(int) Normal]; }
        }

        //public uint StructAlignment { get { return Math.Max(Math.Max(Math.Max(Math.Max(SkinSize, ColorSize), TextureSize), PositionSize), NormalSize); } }
        public uint StructAlignment
        {
            get
            {
                //return Math.Max(Math.Max(Math.Max(Math.Max(SkinSize, ColorSize), TextureSize), PositionSize), NormalSize);
                return (uint) MathUtils.Max(SkinSize, ColorSize, TextureSize, PositionSize, NormalSize);
            }
        }

        public int GetMaxAlignment()
        {
            return (int) MathUtils.Max(SkinSize, ColorSize, TextureSize, PositionSize, NormalSize);
        }

        public int GetVertexSize()
        {
            int Size = 0;
            Size = (int) MathUtils.NextAligned(Size, SkinSize);
            Size += RealSkinningWeightCount * SkinSize;
            Size = (int) MathUtils.NextAligned(Size, TextureSize);
            Size += NormalCount * TextureSize;
            Size = (int) MathUtils.NextAligned(Size, ColorSize);
            Size += 1 * ColorSize;
            Size = (int) MathUtils.NextAligned(Size, NormalSize);
            Size += 3 * NormalSize;
            Size = (int) MathUtils.NextAligned(Size, PositionSize);
            Size += 3 * PositionSize;

            var AlignmentSize = GetMaxAlignment();
            //Size = (uint)((Size + AlignmentSize - 1) & ~(AlignmentSize - 1));
            Size = (int) MathUtils.NextAligned(Size, (uint) AlignmentSize);
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

        public int GetVertexSetMorphSize()
        {
            return GetVertexSize() * MorphingVertexCount;
        }

        public override string ToString()
        {
            return this.ToStringDefault();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
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