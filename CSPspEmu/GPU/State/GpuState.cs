using System;
using System.Numerics;
using System.Runtime.InteropServices;
using CSharpPlatform;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu.State
{
    public class GlobalGpuState
    {
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2048)]
    public unsafe struct GpuStateStruct
    {
        public uint BaseAddress;
        public uint BaseOffset;

        public uint GetAddressRelativeToBase(uint relativeAddress) => BaseAddress | relativeAddress;

        public uint GetAddressRelativeToBaseOffset(uint relativeAddress) =>
            (BaseAddress | relativeAddress) + BaseOffset;

        public uint VertexAddress;
        public uint IndexAddress;
        public bool ToggleUpdateState;

        /// <summary>When set, this will changes the Draw behaviour.</summary>
        public bool ClearingMode;

        public ScreenBufferStateStruct DrawBufferState;
        public ScreenBufferStateStruct DepthBufferState;
        public TextureTransferStateStruct TextureTransferState;

        public ViewportStruct Viewport;
        public PointS Offset;

        /// <summary>A set of flags related to the clearing mode. Generally which buffers to clear.</summary>
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
        public ColorStruct FixColorSource;

        public ColorStruct FixColorDestination;
        public TextureMappingStateStruct TextureMappingState;
        public ShadingModelEnum ShadeModel;
        public fixed sbyte DitherMatrix[16];
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
        /// <summary>Alpha Test Enable (GL_ALPHA_TEST) glAlphaFunc(GL_GREATER, 0.03f);</summary>
        public bool Enabled;

        /// <summary>TestFunction.GU_ALWAYS</summary>
        public TestFunctionEnum Function;

        /// <summary />
        public byte Value;

        /// <summary>0xFF</summary>
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
        public bool Enabled;
        public BlendingOpEnum Equation;
        public GuBlendingFactorSource FunctionSource;
        public GuBlendingFactorDestination FunctionDestination;
        public ColorfStruct FixColorSource;
        public ColorfStruct FixColorDestination;
        public OutputPixel ColorMask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClipPlaneStateStruct
    {
        public bool Enabled;
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
        public bool Enabled;
        public OutputPixel Ref;
        public OutputPixel Mask;
        public ColorTestFunctionEnum Function;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DepthTestStateStruct
    {
        public bool Enabled;
        public TestFunctionEnum Function;
        public float RangeNear;
        public float RangeFar;
        public ushort Mask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DitheringStateStruct
    {
        public bool Enabled;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FogStateStruct
    {
        public bool Enabled;
        public ColorfStruct Color;
        public float Dist;
        public float End;
        public float Density;
        public int Mode;
        public int Hint;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LightingStateStruct
    {
        public bool Enabled;
        public ColorfStruct AmbientModelColor;
        public ColorfStruct DiffuseModelColor;
        public ColorfStruct SpecularModelColor;
        public ColorfStruct EmissiveModelColor;
        public ColorfStruct AmbientLightColor;
        public float SpecularPower;
        public LightStateStruct Light0, Light1, Light2, Light3;
        public LightComponentsSet MaterialColorComponents;
        public LightModelEnum LightModel;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AttenuationStruct
    {
        public float Constant;
        public float Linear;
        public float Quadratic;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LightStateStruct
    {
        public bool Enabled;
        public LightTypeEnum Type;
        public LightModelEnum Kind;
        public Vector4 Position;
        public Vector4 SpotDirection;
        public AttenuationStruct Attenuation;
        public float SpotExponent;
        public float SpotCutoff;
        public ColorfStruct AmbientColor;
        public ColorfStruct DiffuseColor;
        public ColorfStruct SpecularColor;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LineSmoothStateStruct
    {
        public bool Enabled;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LogicalOperationStateStruct
    {
        public bool Enabled;
        public LogicalOperationEnum Operation;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MorphingStateStruct
    {
        public fixed float MorphWeight[8];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PatchCullingStateStruct
    {
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

        public uint Address => 0x04000000 | ((uint) HighAddress << 24) | LowAddress;

        public int BytesPerPixel
        {
            get
            {
                switch (Format)
                {
                    case GuPixelFormats.Rgba5650:
                    case GuPixelFormats.Rgba5551:
                    case GuPixelFormats.Rgba4444: return 2;
                    case GuPixelFormats.Rgba8888: return 4;
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

        public GpuMatrix4X3Struct BoneMatrix0,
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
        public bool Enabled;
        public TestFunctionEnum Function;
        public byte FunctionRef;
        public byte FunctionMask;
        public StencilOperationEnum OperationFail;
        public StencilOperationEnum OperationZFail;
        public StencilOperationEnum OperationZPass;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureMappingStateStruct
    {
        public bool Enabled;
        public GpuMatrix4X4Struct Matrix;
        public ColorbStruct TextureEnviromentColor;
        public TextureStateStruct TextureState;
        public ClutStateStruct UploadedClutState;
        public ClutStateStruct ClutState;
        public TextureMapMode TextureMapMode;
        public TextureProjectionMapMode TextureProjectionMapMode;

        public short ShadeU;
        public short ShadeV;
        public TextureLevelMode LevelMode;
        public float MipmapBias;
        public float SlopeLevel;

        public byte GetTextureComponentsCount()
        {
            byte components = 2;
            switch (TextureMapMode)
            {
                case TextureMapMode.GuTextureCoords:
                    break;
                case TextureMapMode.GuTextureMatrix:
                    switch (TextureProjectionMapMode)
                    {
                        case TextureProjectionMapMode.GuNormal:
                            components = 3;
                            break;
                        case TextureProjectionMapMode.GuNormalizedNormal:
                            components = 3;
                            break;
                        case TextureProjectionMapMode.GuPosition:
                            components = 3;
                            break;
                        case TextureProjectionMapMode.GuUv:
                            components = 2;
                            break;
                    }
                    break;
                case TextureMapMode.GuEnvironmentMap:
                    break;
            }
            return components;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureStateStruct
    {
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
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureTransferStateStruct
    {
        public enum TexelSizeEnum : ushort
        {
            Bit16 = 0,
            Bit32 = 1
        }

        public int BytesPerPixel => (TexelSize == TexelSizeEnum.Bit16) ? 2 : 4;
        public int SourceLineWidthInBytes => SourceLineWidth * BytesPerPixel;
        public int DestinationLineWidthInBytes => DestinationLineWidth * BytesPerPixel;
        public int WidthInBytes => Width * BytesPerPixel;

        public PspPointer SourceAddress, DestinationAddress;
        public ushort SourceLineWidth, DestinationLineWidth;
        public ushort SourceX, SourceY, DestinationX, DestinationY;
        public ushort Width, Height;
        public TexelSizeEnum TexelSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
#pragma warning disable 660,661
    public struct VertexTypeStruct
#pragma warning restore 660,661
    {
        public bool Equals(VertexTypeStruct other) => ReversedNormal == other.ReversedNormal &&
                                                      NormalCount == other.NormalCount && Value == other.Value;

        /// <summary />
        public bool ReversedNormal;

        /// <summary />
        public byte NormalCount;

        /// <summary />
        public uint Value;

        public static bool operator ==(VertexTypeStruct a, VertexTypeStruct b)
        {
            return (a.ReversedNormal == b.ReversedNormal) && (a.NormalCount == b.NormalCount) && (a.Value == b.Value);
        }

        public static bool operator !=(VertexTypeStruct a, VertexTypeStruct b)
        {
            return !(a == b);
        }

        public static readonly int[] TypeSizeTable = {0, sizeof(byte), sizeof(short), sizeof(float)};
        public static readonly int[] ColorSizeTable = {0, 1, 1, 1, 2, 2, 2, 4};

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

        public bool HasTexture => Texture != NumericEnum.Void;

        public NumericEnum Texture
        {
            get => (NumericEnum) BitUtils.Extract(Value, 0, 2);
            set => BitUtils.Insert(ref Value, 0, 2, (uint) value);
        }

        public bool HasColor => Color != ColorEnum.Void;

        public ColorEnum Color
        {
            get => (ColorEnum) BitUtils.Extract(Value, 2, 3);
            set => BitUtils.Insert(ref Value, 2, 3, (uint) value);
        }

        public bool HasNormal => Normal != NumericEnum.Void;

        public NumericEnum Normal
        {
            get => (NumericEnum) BitUtils.Extract(Value, 5, 2);
            set => BitUtils.Insert(ref Value, 5, 2, (uint) value);
        }

        public bool HasPosition => Position != NumericEnum.Void;

        public NumericEnum Position
        {
            get => (NumericEnum) BitUtils.Extract(Value, 7, 2);
            set => BitUtils.Insert(ref Value, 7, 2, (uint) value);
        }

        public bool HasIndex => Index != IndexEnum.Void;

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
            var size = 0;
            size = (int) MathUtils.NextAligned(size, SkinSize);
            size += RealSkinningWeightCount * SkinSize;
            size = (int) MathUtils.NextAligned(size, TextureSize);
            size += NormalCount * TextureSize;
            size = (int) MathUtils.NextAligned(size, ColorSize);
            size += 1 * ColorSize;
            size = (int) MathUtils.NextAligned(size, NormalSize);
            size += 3 * NormalSize;
            size = (int) MathUtils.NextAligned(size, PositionSize);
            size += 3 * PositionSize;

            var alignmentSize = GetMaxAlignment();
            //Size = (uint)((Size + AlignmentSize - 1) & ~(AlignmentSize - 1));
            size = (int) MathUtils.NextAligned(size, (uint) alignmentSize);
            //Console.WriteLine("Size:" + Size);
            return size;
        }

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
        public GpuMatrix4X4Struct ProjectionMatrix;
        public GpuMatrix4X3Struct WorldMatrix;
        public GpuMatrix4X3Struct ViewMatrix;
        public TransformModeEnum TransformMode;
        public VertexTypeStruct Type;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GpuState
    {
        public GpuStateStruct Value;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PointI
    {
        public uint X, Y;

        public override string ToString()
        {
            return this.ToStringDefault();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PointS
    {
        public short X, Y;

        public override string ToString()
        {
            return this.ToStringDefault();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ViewportStruct
    {
        public Vector4 Position;
        public Vector4 Scale;
        public PointS RegionTopLeft;
        public PointS RegionBottomRight;

        public PointS RegionSize => new PointS()
        {
            X = (short) (RegionBottomRight.X - RegionTopLeft.X + 1),
            Y = (short) (RegionBottomRight.Y - RegionTopLeft.Y + 1),
        };

        public override string ToString() => this.ToStringDefault();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorStruct
    {
        public byte iRed, iGreen, iBlue, iAlpha;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorbStruct
    {
        public byte Red, Green, Blue, Alpha;

        public void SetRGB_A1(uint params24) => Alpha = 0xFF;

        public void SetRgb(uint params24)
        {
            Red = (byte) ((params24 >> 0) & 0xFF);
            Green = (byte) ((params24 >> 8) & 0xFF);
            Blue = (byte) ((params24 >> 16) & 0xFF);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorfStruct
    {
        public float Red, Green, Blue, Alpha;

        public void SetRGB_A1(uint params24)
        {
            SetRgb(params24);
            Alpha = 1.0f;
        }

        public void SetRgb(uint params24)
        {
            //Console.WriteLine(Params24);
            Red = ((params24 >> 0) & 0xFF) / 255.0f;
            Green = ((params24 >> 8) & 0xFF) / 255.0f;
            Blue = ((params24 >> 16) & 0xFF) / 255.0f;
        }

        public void SetA(uint params24) => Alpha = ((params24 >> 0) & 0xFF) / 255.0f;

        public override string ToString() => $"Colorf(R={Red}, G={Green}, B={Blue}, A={Alpha})";

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public bool IsColorf(float r, float g, float b) => r == Red && g == Green && b == Blue;

        public static ColorfStruct operator +(ColorfStruct left, ColorfStruct right) => new ColorfStruct
        {
            Red = left.Red + right.Red,
            Green = left.Green + right.Green,
            Blue = left.Blue + right.Blue,
            Alpha = left.Alpha + right.Alpha,
        };


        public Vector4 ToVector4() => new Vector4(Red, Green, Blue, Alpha);
    }

    public enum TransformModeEnum : byte
    {
        Normal = 0,
        Raw = 1,
    }

    public enum GuPrimitiveType : byte
    {
        Points = 0,
        Lines = 1,
        LineStrip = 2,
        Triangles = 3,
        TriangleStrip = 4,
        TriangleFan = 5,
        Sprites = 6,
        ContinuePreviousPrim = 7,
    }

    public enum TextureColorComponent : byte
    {
        Rgb = 0, // GU_TCC_RGB
        Rgba = 1, // GU_TCC_RGBA
    }

    public enum TextureEffect : byte
    {
        Modulate = 0, // GU_TFX_MODULATE
        Decal = 1, // GU_TFX_DECAL
        Blend = 2, // GU_TFX_BLEND
        Replace = 3, // GU_TFX_REPLACE
        Add = 4, // GU_TFX_ADD
    }

    public enum TextureFilter : byte
    {
        Nearest = 0,
        Linear = 1,

        NearestMipmapNearest = 4,
        LinearMipmapNearest = 5,
        NearestMipmapLinear = 6,
        LinearMipmapLinear = 7,
    }

    public enum WrapMode : byte
    {
        Repeat = 0,
        Clamp = 1,
    }

    public enum LogicalOperationEnum : byte
    {
        Clear = 0,
        And = 1,
        AndReverse = 2,
        Copy = 3,
        AndInverted = 4,
        Noop = 5,
        Xor = 6,
        Or = 7,
        NotOr = 8,
        Equiv = 9,
        Inverted = 10,
        OrReverse = 11,
        CopyInverted = 12,
        OrInverted = 13,
        NotAnd = 14,
        Set = 15
    }

    public enum StencilOperationEnum : byte
    {
        Keep = 0,
        Zero = 1,
        Replace = 2,
        Invert = 3,
        Increment = 4,
        Decrement = 5,
    }

    public enum ShadingModelEnum : byte
    {
        Flat = 0,
        Smooth = 1,
    }

    [Flags]
    public enum ClearBufferSet : byte
    {
        ColorBuffer = 1,
        StencilBuffer = 2,
        DepthBuffer = 4,
        FastClear = 16
    }

    public enum BlendingOpEnum : byte
    {
        Add = 0,
        Substract = 1,
        ReverseSubstract = 2,
        Min = 3,
        Max = 4,
        Abs = 5,
    }

    public enum GuBlendingFactorSource : byte
    {
        // Source
        GuSrcColor = 0,
        GuOneMinusSrcColor = 1,
        GuSrcAlpha = 2,
        GuOneMinusSrcAlpha = 3,

        // Both?
        GuFix = 10
    }

    public enum GuBlendingFactorDestination : byte
    {
        // Dest
        GuDstColor = 0,
        GuOneMinusDstColor = 1,
        GuDstAlpha = 4,
        GuOneMinusDstAlpha = 5,

        // Both?
        GuFix = 10
    }

    public enum TestFunctionEnum : byte
    {
        Never = 0,
        Always = 1,
        Equal = 2,
        NotEqual = 3,
        Less = 4,
        LessOrEqual = 5,
        Greater = 6,
        GreaterOrEqual = 7,
    }

    public enum FrontFaceDirectionEnum : byte
    {
        CounterClockWise = 0,
        ClockWise = 1
    }

    public enum ColorTestFunctionEnum : byte
    {
        GuNever,
        GuAlways,
        GuEqual,
        GuNotequal,
    }

    [Flags]
    public enum LightComponentsSet : byte
    {
        Ambient = 1,
        Diffuse = 2,
        Specular = 4,
        AmbientAndDiffuse = Ambient | Diffuse,
        DiffuseAndSpecular = Diffuse | Specular,
        UnknownLightComponent = 8,
    }

    public enum LightTypeEnum : byte
    {
        Directional = 0,
        PointLight = 1,
        SpotLight = 2,
    }

    public enum LightModelEnum : byte
    {
        SingleColor = 0,
        SeparateSpecularColor = 1,
    }

    public enum TextureMapMode : byte
    {
        GuTextureCoords = 0,
        GuTextureMatrix = 1,
        GuEnvironmentMap = 2,
    }

    public enum TextureProjectionMapMode : byte
    {
        GuPosition = 0, // TMAP_TEXTURE_PROJECTION_MODE_POSITION - 3 texture components
        GuUv = 1, // TMAP_TEXTURE_PROJECTION_MODE_TEXTURE_COORDINATES - 2 texture components 
        GuNormalizedNormal = 2, // TMAP_TEXTURE_PROJECTION_MODE_NORMALIZED_NORMAL - 3 texture components
        GuNormal = 3, // TMAP_TEXTURE_PROJECTION_MODE_NORMAL - 3 texture components
    }
}