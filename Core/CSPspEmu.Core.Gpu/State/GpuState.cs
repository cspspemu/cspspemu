using System;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu.State
{
    public class GlobalGpuState
    {
    }

    /*
    public class GlobalGpuState
    {
        public uint GetAddressRelativeToBase(uint RelativeAddress)
        {
            return (uint)(this.Base | RelativeAddress);
        }

        public uint GetAddressRelativeToBaseOffset(uint RelativeAddress)
        {
            return (uint)((this.Base | RelativeAddress) + this.BaseOffset);
        }

        public uint Base;
        public uint BaseOffset;
    }
    */

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2048)]
    public unsafe struct GpuStateStruct
    {
        public uint BaseAddress;
        public uint BaseOffset;

        public uint GetAddressRelativeToBase(uint RelativeAddress)
        {
            return (uint) (this.BaseAddress | RelativeAddress);
        }

        public uint GetAddressRelativeToBaseOffset(uint RelativeAddress)
        {
            return (uint) ((this.BaseAddress | RelativeAddress) + this.BaseOffset);
        }


        public uint VertexAddress;
        public uint IndexAddress;
        public bool ToggleUpdateState;

        /// <summary>
        /// When set, this will changes the Draw behaviour.
        /// </summary>
        public bool ClearingMode;

        public ScreenBufferStateStruct DrawBufferState;
        public ScreenBufferStateStruct DepthBufferState;
        public TextureTransferStateStruct TextureTransferState;

        public ViewportStruct Viewport;
        public PointS Offset;

        /// <summary>
        /// A set of flags related to the clearing mode. Generally which buffers to clear.
        /// </summary>
        public ClearBufferSet ClearFlags;

        // Sub States.
        public VertexStateStruct VertexState;

        public BackfaceCullingStateStruct BackfaceCullingState;

        public FogStateStruct FogState;
        public BlendingStateStruct BlendingState;
        public StencilStateStruct StencilState;
        public AlphaTestStateStruct AlphaTestState;
        public LogicalOperationStateStruct LogicalOperationState;
        public DepthTestStateStruct DepthTestState;
        public LightingStateStruct LightingState;
        public MorphingStateStruct MorphingState;
        public DitheringStateStruct DitheringState;
        public LineSmoothStateStruct LineSmoothState;
        public ClipPlaneStateStruct ClipPlaneState;
        public PatchCullingStateStruct PatchCullingState;
        public SkinningStateStruct SkinningState;
        public ColorTestStateStruct ColorTestState;

        public PatchStateStruct PatchState;

        // State.
        public ColorStruct FixColorSource, FixColorDestination;

        public TextureMappingStateStruct TextureMappingState;

        /// <summary>
        /// 
        /// </summary>
        public ShadingModelEnum ShadeModel;

        public fixed sbyte DitherMatrix[16];

        /*
        public static void Init(GpuStateStruct* GpuState)
        {
            PointerUtils.Memset((byte*)GpuState, 0, sizeof(GpuStateStruct));

            GpuState->SkinningState.BoneMatrix0.Init();
            GpuState->SkinningState.BoneMatrix1.Init();
            GpuState->SkinningState.BoneMatrix2.Init();
            GpuState->SkinningState.BoneMatrix3.Init();
            GpuState->SkinningState.BoneMatrix4.Init();
            GpuState->SkinningState.BoneMatrix5.Init();
            GpuState->SkinningState.BoneMatrix6.Init();
            GpuState->SkinningState.BoneMatrix7.Init();
        }
        */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PatchStateStruct
    {
        public byte DivS;
        public byte DivT;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AlphaTestStateStruct
    {
        /// <summary>
        /// Alpha Test Enable (GL_ALPHA_TEST) glAlphaFunc(GL_GREATER, 0.03f);
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// TestFunction.GU_ALWAYS
        /// </summary>
        public TestFunctionEnum Function;

        /// <summary>
        /// 
        /// </summary>
        public byte Value;

        /// <summary>
        /// 0xFF
        /// </summary>
        public byte Mask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BackfaceCullingStateStruct
    {
        /// <summary>Backface Culling Enable (GL_CULL_FACE)</summary>
        public bool Enabled;

        /// <summary />
        public FrontFaceDirectionEnum FrontFaceDirection;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlendingStateStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public BlendingOpEnum Equation;

        /// <summary>
        /// 
        /// </summary>
        public GuBlendingFactorSource FunctionSource;

        /// <summary>
        /// 
        /// </summary>
        public GuBlendingFactorDestination FunctionDestination;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct FixColorSource;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct FixColorDestination;

        /// <summary>
        /// 
        /// </summary>
        public OutputPixel ColorMask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClipPlaneStateStruct
    {
        /// <summary>
        /// Clip Plane Enable (GL_CLIP_PLANE0)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public GpuRectStruct Scissor;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ClutStateStruct
    {
        public uint Address;
        public int Shift;
        public int Mask;
        public int Start;
        public GuPixelFormats PixelFormat;
        public byte* Data;
        public int NumberOfColors;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorTestStateStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public OutputPixel Ref;

        /// <summary>
        /// 
        /// </summary>
        public OutputPixel Mask;

        /// <summary>
        /// 
        /// </summary>
        public ColorTestFunctionEnum Function;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DepthTestStateStruct
    {
        /// <summary>
        /// depth (Z) Test Enable (GL_DEPTH_TEST)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// TestFunction.GU_ALWAYS
        /// </summary>
        public TestFunctionEnum Function;

        /// <summary>
        /// 0.0 - 1.0
        /// </summary>
        public float RangeNear;

        /// <summary>
        /// 0.0 - 1.0
        /// </summary>
        public float RangeFar;

        /// <summary>
        /// 
        /// </summary>
        public ushort Mask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DitheringStateStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FogStateStruct
    {
        /// <summary>
        /// FOG Enable (GL_FOG)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct Color;

        /// <summary>
        /// 
        /// </summary>
        public float Dist;

        /// <summary />
        public float End;

        /// <summary>Default Value: 0.1</summary>
        public float Density;

        /// <summary />
        public int Mode;

        /// <summary />
        public int Hint;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LightingStateStruct
    {
        /// <summary>
        /// Lighting Enable (GL_LIGHTING)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct AmbientModelColor;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct DiffuseModelColor;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct SpecularModelColor;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct EmissiveModelColor;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct AmbientLightColor;

        /// <summary>
        /// 
        /// </summary>
        public float SpecularPower;

        /// <summary>
        /// 
        /// </summary>
        public LightStateStruct Light0, Light1, Light2, Light3;

        /// <summary>
        /// 
        /// </summary>
        public LightComponentsSet MaterialColorComponents;

        /// <summary>
        /// 
        /// </summary>
        public LightModelEnum LightModel;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AttenuationStruct
    {
        public float Constant;
        public float Linear;
        public float Quadratic;
    }

    public struct Vector4fRef
    {
        public float X, Y, Z, W;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LightStateStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public LightTypeEnum Type;

        /// <summary>
        /// 
        /// </summary>
        public LightModelEnum Kind;

        /// <summary>
        /// 
        /// </summary>
        public Vector4fRef Position;

        /// <summary>
        /// 
        /// </summary>
        public Vector4fRef SpotDirection;

        /// <summary>
        /// 
        /// </summary>
        public AttenuationStruct Attenuation;

        /// <summary>
        /// 
        /// </summary>
        public float SpotExponent;

        /// <summary>
        /// 
        /// </summary>
        public float SpotCutoff;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct AmbientColor;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct DiffuseColor;

        /// <summary>
        /// 
        /// </summary>
        public ColorfStruct SpecularColor;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LineSmoothStateStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LogicalOperationStateStruct
    {
        /// <summary>
        /// Logical Operation Enable (GL_COLOR_LOGIC_OP)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// LogicalOperation.GU_COPY
        /// </summary>
        public LogicalOperationEnum Operation;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MorphingStateStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public float MorphWeight0;

        /// <summary>
        /// 
        /// </summary>
        public float MorphWeight1;

        /// <summary>
        /// 
        /// </summary>
        public float MorphWeight2;

        /// <summary>
        /// 
        /// </summary>
        public float MorphWeight3;

        /// <summary>
        /// 
        /// </summary>
        public float MorphWeight4;

        /// <summary>
        /// 
        /// </summary>
        public float MorphWeight5;

        /// <summary>
        /// 
        /// </summary>
        public float MorphWeight6;

        /// <summary>
        /// 
        /// </summary>
        public float MorphWeight7;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PatchCullingStateStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled;

        public bool FaceFlag;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ScreenBufferStateStruct
    {
        public uint Width;
        public GuPixelFormats Format;

        public byte HighAddress;
        public uint LowAddress;

        public uint LoadAddress;
        public uint StoreAddress;

        public uint Address
        {
            get { return 0x04000000 | ((uint) HighAddress << 24) | LowAddress; }
        }

        //uint Width = 512;
        //PspDisplay.PixelFormats Format = PspDisplay.PixelFormats.RGBA_8888;
        /*
        union {
            uint _address;
            struct { mixin(bitfields!(
                uint, "lowAddress" , 24,
                uint, "highAddress", 8
            )); }
        }
        uint address(uint _address) { return this._address = _address; }
        uint address() { return (0x04_000000 | this._address); }
        uint addressEnd() { return address + width * 272 * pixelSize; }
        uint pixelSize() { return PixelFormatSizeMul[format]; }
        ubyte[] row(void* ptr, int row) {
            int rowsize = PixelFormatSize(format, width);
            return ((cast(ubyte *)ptr) + rowsize * row)[0..rowsize];
        }
        bool isAnyAddressInBuffer(uint[] ptrList) {
            foreach (ptr; ptrList) {
                if ((ptr >= address) && (ptr < addressEnd)) return true;
            }
            return false;
        }
        */

        public int BytesPerPixel
        {
            get
            {
                switch (Format)
                {
                    case GuPixelFormats.RGBA_5650:
                    case GuPixelFormats.RGBA_5551:
                    case GuPixelFormats.RGBA_4444: return 2;
                    case GuPixelFormats.RGBA_8888: return 4;
                    default:
                        throw (new InvalidOperationException(
                            "ScreenBufferStateStruct.BytesPerPixel : Invalid Format : " + Format));
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SkinningStateStruct
    {
        public int CurrentBoneIndex;

        public GpuMatrix4x3Struct BoneMatrix0,
            BoneMatrix1,
            BoneMatrix2,
            BoneMatrix3,
            BoneMatrix4,
            BoneMatrix5,
            BoneMatrix6,
            BoneMatrix7;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StencilStateStruct
    {
        /// <summary>
        /// Stencil Test Enable (GL_STENCIL_TEST)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public TestFunctionEnum Function;

        /// <summary>
        /// 
        /// </summary>
        public byte FunctionRef;

        /// <summary>
        /// 0xFF
        /// </summary>
        public byte FunctionMask;

        /// <summary>
        /// 
        /// </summary>
        public StencilOperationEnum OperationFail;

        /// <summary>
        /// 
        /// </summary>
        public StencilOperationEnum OperationZFail;

        /// <summary>
        /// 
        /// </summary>
        public StencilOperationEnum OperationZPass;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureMappingStateStruct
    {
        /// <summary>
        /// Texture Mapping Enable (GL_TEXTURE_2D)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public GpuMatrix4x4Struct Matrix;

        /// <summary>
        /// 
        /// </summary>
        public ColorbStruct TextureEnviromentColor;

        /// <summary>
        /// 
        /// </summary>
        public TextureStateStruct TextureState;

        /// <summary>
        /// 
        /// </summary>
        public ClutStateStruct UploadedClutState;

        /// <summary>
        /// 
        /// </summary>
        public ClutStateStruct ClutState;

        /// <summary>
        /// 
        /// </summary>
        public TextureMapMode TextureMapMode;

        /// <summary>
        /// 
        /// </summary>
        public TextureProjectionMapMode TextureProjectionMapMode;

        public short ShadeU;
        public short ShadeV;
        public TextureLevelMode LevelMode;
        public float MipmapBias;
        public float SlopeLevel;

        public byte GetTextureComponentsCount()
        {
            byte Components = 2;
            switch (TextureMapMode)
            {
                case TextureMapMode.GU_TEXTURE_COORDS:
                    break;
                case TextureMapMode.GU_TEXTURE_MATRIX:
                    switch (TextureProjectionMapMode)
                    {
                        case TextureProjectionMapMode.GU_NORMAL:
                            Components = 3;
                            break;
                        case TextureProjectionMapMode.GU_NORMALIZED_NORMAL:
                            Components = 3;
                            break;
                        case TextureProjectionMapMode.GU_POSITION:
                            Components = 3;
                            break;
                        case TextureProjectionMapMode.GU_UV:
                            Components = 2;
                            break;
                    }
                    break;
                case TextureMapMode.GU_ENVIRONMENT_MAP:
                    break;
            }
            return Components;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureStateStruct
    {
        /// <summary>
        /// Mimaps
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MipmapState
        {
            /// <summary>
            /// Pointer 
            /// </summary>
            public uint Address;

            /// <summary>
            /// Data width of the image.
            /// With will be bigger. For example:
            /// Bufferwidth = 480, Width = 512, Height = 512
            /// Texture is 512x512 but there is data just for 480x512
            /// </summary>
            public ushort BufferWidth;

            /// <summary>
            /// Texture Width
            /// </summary>
            public ushort TextureWidth;

            /// <summary>
            /// Texture Height
            /// </summary>
            public ushort TextureHeight;
        }

        /// <summary>
        /// Is texture swizzled?
        /// </summary>
        public bool Swizzled;

        /// <summary>
        /// Mipmaps share clut?
        /// </summary>
        public bool MipmapShareClut;

        /// <summary>
        /// Levels of mipmaps
        /// </summary>
        public int MipmapMaxLevel;

        /// <summary>
        /// Format of the texture data.
        /// Texture Data mode
        /// </summary>
        public GuPixelFormats PixelFormat;

        /// <summary>
        /// MipmapState list
        /// </summary>
        public MipmapState Mipmap0;

        public MipmapState Mipmap1;
        public MipmapState Mipmap2;
        public MipmapState Mipmap3;
        public MipmapState Mipmap4;
        public MipmapState Mipmap5;
        public MipmapState Mipmap6;
        public MipmapState Mipmap7;

        /// <summary>
        /// TextureFilter when drawing the texture scaled
        /// </summary>
        public TextureFilter FilterMinification;

        /// <summary>
        /// TextureFilter when drawing the texture scaled
        /// </summary>
        public TextureFilter FilterMagnification;

        /// <summary>
        /// Wrap mode when specifying texture coordinates beyond texture size
        /// </summary>
        public WrapMode WrapU;

        /// <summary>
        /// Wrap mode when specifying texture coordinates beyond texture size
        /// </summary>
        public WrapMode WrapV;

        /// <summary>
        /// 
        /// </summary>
        public float ScaleU;

        /// <summary />
        public float ScaleV;

        /// <summary />
        public float OffsetU;

        /// <summary />
        public float OffsetV;

        /// <summary>Effects:</summary>
        public bool Fragment2X;

        /// <summary />
        public TextureEffect Effect;

        /// <summary />
        public TextureColorComponent ColorComponent;

        /*
        public int mipmapRealWidth(int mipmap = 0) { return PixelFormatSize(format, mipmaps[mipmap].buffer_width); }
        public int mipmapTotalSize(int mipmap = 0) { return mipmapRealWidth(mipmap) * mipmaps[mipmap].height; }

        public string hash() { return cast(string)TA(this); }
        //string toString() { return std.string.format("TextureState(addr=%08X, size(%dx%d), bwidth=%d, format=%d, swizzled=%d)", address, width, height, buffer_width, format, swizzled); }

        public int address() { return mipmaps->address; }
        public int buffer_width() { return mipmaps->buffer_width; }
        public int width() { return mipmaps->width; }
        public int height() { return mipmaps->height; }
        public bool hasPalette() { return (format >= PixelFormats.GU_PSM_T4 && format <= PixelFormats.GU_PSM_T32); }
        public uint paletteRequiredComponents() { return hasPalette ? (1 << (4 + (format - PixelFormats.GU_PSM_T4))) : 0; }
        */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureTransferStateStruct
    {
        public enum TexelSizeEnum : ushort
        {
            BIT_16 = 0,
            BIT_32 = 1
        }
        //enum TexelSize { BIT_32 = 0, BIT_16 = 1 }

        public int BytesPerPixel => (TexelSize == TexelSizeEnum.BIT_16) ? 2 : 4;
        public int SourceLineWidthInBytes => (int) (SourceLineWidth * BytesPerPixel);
        public int DestinationLineWidthInBytes => (int) (DestinationLineWidth * BytesPerPixel);
        public int WidthInBytes => (int) (Width * BytesPerPixel);

        public PspPointer SourceAddress, DestinationAddress;
        public ushort SourceLineWidth, DestinationLineWidth;
        public ushort SourceX, SourceY, DestinationX, DestinationY;
        public ushort Width, Height;
        public TexelSizeEnum TexelSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexTypeStruct
    {
        /// <summary />
        public bool ReversedNormal;

        /// <summary />
        public byte NormalCount;

        /// <summary />
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

        public bool HasWeight => Weight != NumericEnum.Void;

        public NumericEnum Weight
        {
            get => (NumericEnum) BitUtils.Extract(Value, 9, 2);
            set => BitUtils.Insert(ref Value, 9, 2, (uint) value);
        }

        public bool HasTexture => Texture != VertexTypeStruct.NumericEnum.Void;

        public NumericEnum Texture
        {
            get => (NumericEnum) BitUtils.Extract(Value, 0, 2);
            set => BitUtils.Insert(ref Value, 0, 2, (uint) value);
        }

        public bool HasColor => Color != VertexTypeStruct.ColorEnum.Void;

        public ColorEnum Color
        {
            get => (ColorEnum) BitUtils.Extract(Value, 2, 3);
            set => BitUtils.Insert(ref Value, 2, 3, (uint) value);
        }

        public bool HasNormal => Normal != VertexTypeStruct.NumericEnum.Void;

        public NumericEnum Normal
        {
            get => (NumericEnum) BitUtils.Extract(Value, 5, 2);
            set => BitUtils.Insert(ref Value, 5, 2, (uint) value);
        }

        public bool HasPosition => Position != VertexTypeStruct.NumericEnum.Void;

        public NumericEnum Position
        {
            get => (NumericEnum) BitUtils.Extract(Value, 7, 2);
            set => BitUtils.Insert(ref Value, 7, 2, (uint) value);
        }

        public bool HasIndex => Index != VertexTypeStruct.IndexEnum.Void;

        public IndexEnum Index
        {
            get => (IndexEnum) BitUtils.Extract(Value, 11, 2);
            set => BitUtils.Insert(ref Value, 11, 2, (uint) value);
        }

        public int SkinningWeightCount
        {
            get => (int) BitUtils.Extract(Value, 14, 3);
            set => BitUtils.Insert(ref Value, 14, 3, (uint) value);
        }

        public int MorphingVertexCount
        {
            get => (int) BitUtils.Extract(Value, 18, 2);
            set => BitUtils.Insert(ref Value, 18, 2, (uint) value);
        }

        public bool Transform2D
        {
            get => BitUtils.Extract(Value, 23, 1) != 0;
            set => BitUtils.Insert(ref Value, 23, 1, value ? 1U : 0U);
        }

        public int RealSkinningWeightCount => SkinSize == 0 ? 0 : SkinningWeightCount + 1;

        //public bool HasWeghts { get { return Weight != NumericEnum.Void; } }

        public int SkinSize => TypeSizeTable[(int) Weight];
        public int ColorSize => ColorSizeTable[(int) Color];
        public int TextureSize => TypeSizeTable[(int) Texture];
        public int PositionSize => TypeSizeTable[(int) Position];
        public int NormalSize => TypeSizeTable[(int) Normal];

        //public uint StructAlignment { get { return Math.Max(Math.Max(Math.Max(Math.Max(SkinSize, ColorSize), TextureSize), PositionSize), NormalSize); } }
        public uint StructAlignment => (uint) MathUtils.Max(SkinSize, ColorSize, TextureSize, PositionSize, NormalSize);

        public int GetMaxAlignment() => MathUtils.Max(SkinSize, ColorSize, TextureSize, PositionSize, NormalSize);

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