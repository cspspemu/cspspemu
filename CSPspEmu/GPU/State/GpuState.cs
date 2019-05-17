using System;
using System.Numerics;
using System.Runtime.InteropServices;
using CSharpPlatform;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using CSPspEmu.Utils;

namespace CSPspEmu.Core.Gpu.State
{
    public class GlobalGpuState
    {
    }

    public unsafe class GpuStateData
    {
        public uint* Data;
        public Span<uint> Span => new Span<uint>(Data, GpuStateStruct.StructSizeInWords);

        public GpuStateData(uint* data)
        {
            Data = data;
        }
        
        public uint this[GpuOpCodes op]
        {
            get => Span[(int)op];
            set => Span[(int)op] = value;
        }

        public uint Int(GpuOpCodes op) => (this[op] & 0xFFFFFF);
        public uint Param8(GpuOpCodes op, int offset) => (Int(op) >> offset) & 0xFF;
        public uint Param16(GpuOpCodes op, int offset) => (Int(op) >> offset) & 0xFFFF;
        public uint Param24(GpuOpCodes op) => (this[op] & 0xFFFFFF);
        public uint Extract(GpuOpCodes op, int offset, int bits) => (uint)(this[op] >> offset) & (uint)((1 << bits) - 1);
        public int ExtractSigned(GpuOpCodes op, int offset, int bits)
        {
            var res = (int)Extract(op, offset, bits);
            return res << (32 - bits) >> bits;
        }
        public bool Bool(GpuOpCodes op) => Int(op) != 0;
        public float Float1(GpuOpCodes op) => MathFloat.ReinterpretUIntAsFloat(this[op] << 8);

    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2048)]
    public class GpuStateStruct
    {
        public const int StructSizeInWords = 0x200;
        public const int StructSizeInBytes = StructSizeInWords * 4;
        
        public GpuStateData data;
        
        public ScreenBufferStateStruct DrawBufferState;
        public ScreenBufferStateStruct DepthBufferState;
        public TextureTransferStateStruct TextureTransferState;
        public ViewportStruct Viewport;
        public VertexStateStruct VertexState;
        public BackfaceCullingStateStruct BackfaceCullingState;
        public FogState FogState;
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
        public TextureMappingStateStruct TextureMappingState;

        public GpuStateStruct(GpuStateData data)
        {
            this.data = data;
            
            DrawBufferState = new ScreenBufferStateStruct(data);
            DepthBufferState = new ScreenBufferStateStruct(data);
            TextureTransferState = new TextureTransferStateStruct(data);
            Viewport = new ViewportStruct(data);
            VertexState = new VertexStateStruct(data);
            BackfaceCullingState = new BackfaceCullingStateStruct(data);
            FogState = new FogState(data);
            BlendingState = new BlendingStateStruct();
            StencilState = new StencilStateStruct(data);
            AlphaTestState = new AlphaTestStateStruct();
            LogicalOperationState = new LogicalOperationStateStruct();
            DepthTestState = new DepthTestStateStruct(data);
            LightingState = new LightingStateStruct(data);
            MorphingState = new MorphingStateStruct(data);
            DitheringState = new DitheringStateStruct(data);
            LineSmoothState = new LineSmoothStateStruct(data);
            ClipPlaneState = new ClipPlaneStateStruct(data);
            PatchCullingState = new PatchCullingStateStruct(data);
            SkinningState = new SkinningStateStruct();
            ColorTestState = new ColorTestStateStruct(data);
            PatchState = new PatchStateStruct(data);
            TextureMappingState = new TextureMappingStateStruct(data);
        }

        public uint BaseAddress => ((data.Param24(GpuOpCodes.BASE) << 8) & 0xff000000);
        public uint BaseOffset
        {
            get { return data[GpuOpCodes.OFFSET_ADDR]; }
            set { data[GpuOpCodes.OFFSET_ADDR] = value; }
        }

        public uint GetAddressRelativeToBase(uint relativeAddress) => BaseAddress | relativeAddress;

        public uint GetAddressRelativeToBaseOffset(uint relativeAddress) =>
            (BaseAddress | relativeAddress) + BaseOffset;

        public uint VertexAddress
        {
            get { return data.Int(GpuOpCodes.VADDR); }
            set { data[GpuOpCodes.VADDR] = value; }
        }

        public uint IndexAddress => data.Int(GpuOpCodes.IADDR);
        public bool ToggleUpdateState;

        /// <summary>When set, this will changes the Draw behaviour.</summary>
        public bool ClearingMode => (data.Param24(GpuOpCodes.CLEAR) & 1) != 0;
        public PointS Offset => new PointS((short) data.Extract(GpuOpCodes.OFFSETX, 0, 4), (short) data.Extract(GpuOpCodes.OFFSETY, 0, 4));
        public ClearBufferSet ClearFlags => (ClearBufferSet) data.Param8(GpuOpCodes.CLEAR, 8);
        public ColorStruct FixColorSource;
        public ColorStruct FixColorDestination;
        public ShadingModelEnum ShadeModel => (ShadingModelEnum) data.Int(GpuOpCodes.SHADE);

        private sbyte[] _DitherMatrix = new sbyte[16];
        public sbyte[] DitherMatrix
        {
            get
            {
                for (byte n = 0; n < 4; n++)
                {
                    _DitherMatrix[4 * n + 0] = (sbyte) data.ExtractSigned(GpuOpCodes.DTH0 + n, 4 * 0, 4);
                    _DitherMatrix[4 * n + 1] = (sbyte) data.ExtractSigned(GpuOpCodes.DTH0 + n, 4 * 1, 4);
                    _DitherMatrix[4 * n + 2] = (sbyte) data.ExtractSigned(GpuOpCodes.DTH0 + n, 4 * 2, 4);
                    _DitherMatrix[4 * n + 3] = (sbyte) data.ExtractSigned(GpuOpCodes.DTH0 + n, 4 * 3, 4);
                }

                var o = new sbyte[16];
                return _DitherMatrix;
            }
        }
    }

    public class PatchStateStruct
    {
        private GpuStateData data;

        public PatchStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public byte DivS => (byte)data.Param8(GpuOpCodes.PSUB, 0);
        public byte DivT => (byte)data.Param8(GpuOpCodes.PSUB, 8);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AlphaTestStateStruct
    {
        private GpuStateData data;

        public AlphaTestStateStruct(GpuStateData data) : this()
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.ATE);
        public TestFunctionEnum Function => (TestFunctionEnum) data.Param8(GpuOpCodes.ATST, 0);
        public byte Value => (byte)data.Param8(GpuOpCodes.ATST, 8);
        public byte Mask => (byte)data.Param8(GpuOpCodes.ATST, 16);
    }

    public class BackfaceCullingStateStruct
    {
        public GpuStateData data;

        public BackfaceCullingStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.BCE);
        public FrontFaceDirectionEnum FrontFaceDirection => (FrontFaceDirectionEnum) data.Int(GpuOpCodes.FFACE);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlendingStateStruct
    {
        private GpuStateData data;

        public BlendingStateStruct(GpuStateData data) : this()
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.ABE);
        
        public GuBlendingFactorSource FunctionSource => (GuBlendingFactorSource) ((data.Param24(GpuOpCodes.ALPHA) >> 0) & 0xF);

        public GuBlendingFactorDestination FunctionDestination =>
            (GuBlendingFactorDestination) ((data.Param24(GpuOpCodes.ALPHA) >> 4) & 0xF);

        public BlendingOpEnum Equation => (BlendingOpEnum) ((data.Param24(GpuOpCodes.ALPHA) >> 8) & 0xF);
        
        public ColorfStruct FixColorSource => new ColorfStruct().SetRGB_A1(data.Param24(GpuOpCodes.SFIX));
        public ColorfStruct FixColorDestination => new ColorfStruct().SetRGB_A1(data.Param24(GpuOpCodes.DFIX));
        public OutputPixel ColorMask => OutputPixel.FromRgba(
            (byte)data.Param8(GpuOpCodes.PMSKC, 0),
            (byte)data.Param8(GpuOpCodes.PMSKC, 8),
            (byte)data.Param8(GpuOpCodes.PMSKC, 16),
            (byte)data.Param8(GpuOpCodes.PMSKA, 0)
            );
    }

    public class ClipPlaneStateStruct
    {
        private GpuStateData data;

        public ClipPlaneStateStruct(GpuStateData data)
        {
            this.data = data;
        }


        public bool Enabled => data.Bool(GpuOpCodes.CPE);
        public GpuRectStruct Scissor => new GpuRectStruct(
            (short) data.Extract(GpuOpCodes.SCISSOR1, 0, 10),
            (short) data.Extract(GpuOpCodes.SCISSOR1, 10, 10),
            (short) data.Extract(GpuOpCodes.SCISSOR2, 0, 10),
            (short) data.Extract(GpuOpCodes.SCISSOR2, 10, 10)
            );
    }

    unsafe public class ClutStateStruct
    {
        private GpuStateData data;

        public ClutStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public uint Address => data.Int(GpuOpCodes.CBP) | (data.Int(GpuOpCodes.CBPH) << 24);
        public int NumberOfColors
        {
            get { return (int) (data.Param8(GpuOpCodes.CLOAD, 0) * 8); }
            set { data[GpuOpCodes.CLOAD] = (uint) (value / 8); }
        }

        public GuPixelFormats PixelFormat => (GuPixelFormats) data.Extract(GpuOpCodes.CMODE, 0, 2);
        public int Shift => (int)data.Extract(GpuOpCodes.CMODE, 2, 5);
        public int Mask => (int)data.Extract(GpuOpCodes.CMODE, 8, 8);
        public int Start => (int)data.Extract(GpuOpCodes.CMODE, 16, 5);
        public byte* Data => throw new NotImplementedException();
    }

    public class ColorTestStateStruct
    {
        private GpuStateData data;

        public ColorTestStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.CTE);
        public OutputPixel Ref => OutputPixel.FromRgba(
            (byte) data.Extract(GpuOpCodes.CREF, 8 * 0, 8),
            (byte) data.Extract(GpuOpCodes.CREF, 8 * 1, 8),
            (byte) data.Extract(GpuOpCodes.CREF, 8 * 2, 8),
            0x00
        );
        public OutputPixel Mask => OutputPixel.FromRgba(
            (byte) data.Extract(GpuOpCodes.CMSK, 8 * 0, 8),
            (byte) data.Extract(GpuOpCodes.CMSK, 8 * 1, 8),
            (byte) data.Extract(GpuOpCodes.CMSK, 8 * 2, 8),
            0x00
        );
        public ColorTestFunctionEnum Function => (ColorTestFunctionEnum) data.Extract(GpuOpCodes.CTST, 0, 2);
    }

    public class DepthTestStateStruct
    {
        private GpuStateData data;

        public DepthTestStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.ZTE);
        public TestFunctionEnum Function => (TestFunctionEnum) data.Param8(GpuOpCodes.ZTST, 0);
        public float RangeNear => ((float) data.Param16(GpuOpCodes.FARZ, 0)) / ushort.MaxValue; // @TODO: CHECK INVERTED!
        public float RangeFar => ((float) data.Param16(GpuOpCodes.NEARZ, 0)) / ushort.MaxValue; // @TODO: CHECK INVERTED!
        public ushort Mask => (ushort)data.Param16(GpuOpCodes.ZMSK, 0);
    }

    public class DitheringStateStruct
    {
        public GpuStateData data;

        public DitheringStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.DTE);
    }

    public class FogState
    {
        public GpuStateData data;

        public FogState(GpuStateData data)
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.FGE);
        public ColorfStruct Color => new ColorfStruct().SetRGB_A1(data.Param24(GpuOpCodes.FCOL));
        public float Dist => data.Float1(GpuOpCodes.FDIST);
        public float End => data.Float1(GpuOpCodes.FFAR);
        public float Density;
        public int Mode;
        public int Hint;
    }

    public class LightingStateStruct
    {
        private GpuStateData data;

        public LightingStateStruct(GpuStateData data)
        {
            this.data = data;
            Lights = new LightStateStruct[4];
            for (var n = 0; n < 4; n++) Lights[n] = new LightStateStruct(data, (byte)n);
        }

        public LightStateStruct[] Lights;
        public LightStateStruct Light0 => Lights[0];
        public LightStateStruct Light1 => Lights[1];
        public LightStateStruct Light2 => Lights[2];
        public LightStateStruct Light3 => Lights[3];

        public bool Enabled => data.Bool(GpuOpCodes.LTE);
        public ColorfStruct AmbientModelColor => new ColorfStruct().SetRGB_A1(data.Param24(GpuOpCodes.AMC)).SetA(data.Param24(GpuOpCodes.AMA));
        public ColorfStruct DiffuseModelColor => new ColorfStruct().SetRGB_A1(data.Param24(GpuOpCodes.DMC));
        public ColorfStruct SpecularModelColor => new ColorfStruct().SetRGB_A1(data.Param24(GpuOpCodes.SMC));
        public ColorfStruct EmissiveModelColor => new ColorfStruct().SetRGB_A1(data.Param24(GpuOpCodes.EMC));
        public ColorfStruct AmbientLightColor => new ColorfStruct().SetRGB_A1(data.Param24(GpuOpCodes.ALC)).SetA(data.Param24(GpuOpCodes.ALA));
        public float SpecularPower => data.Float1(GpuOpCodes.SPOW);
        public LightComponentsSet MaterialColorComponents => (LightComponentsSet) data.Param8(GpuOpCodes.CMAT, 0);
        public LightModelEnum LightModel => (LightModelEnum) data.Param8(GpuOpCodes.LMODE, 0);
    }

    public class AttenuationStruct
    {
        private GpuStateData data;
        private byte index;

        public AttenuationStruct(GpuStateData data, byte index)
        {
            this.data = data;
            this.index = index;
        }


        public float Constant => data.Float1(GpuOpCodes.LCA0 + index);
        public float Linear => data.Float1(GpuOpCodes.LLA0 + index);
        public float Quadratic => data.Float1(GpuOpCodes.LQA0 + index);
    }

    public class LightStateStruct
    {
        private GpuStateData data;
        private byte index;

        public LightStateStruct(GpuStateData data, byte index)
        {
            this.data = data;
            this.index = index;
        }

        public bool Enabled => data.Bool(GpuOpCodes.LTE0 + index);
        public LightTypeEnum Type => (LightTypeEnum) data.Param8(GpuOpCodes.LT0 + index, 8);
        public LightModelEnum Kind => (LightModelEnum) data.Param8(GpuOpCodes.LT0 + index, 0);
        public Vector4 Position => new Vector4(
            data.Float1(GpuOpCodes.LXP0 + index),
            data.Float1(GpuOpCodes.LYP0 + index),
            data.Float1(GpuOpCodes.LZP0 + index),
            (Type == LightTypeEnum.Directional) ? 0 : 1
        );
        public Vector4 SpotDirection=> new Vector4(
            data.Float1(GpuOpCodes.LXD0 + index),
            data.Float1(GpuOpCodes.LYD0 + index),
            data.Float1(GpuOpCodes.LZD0 + index),
            1
        );
        public AttenuationStruct Attenuation;
        public float SpotExponent => data.Float1(GpuOpCodes.SPOTEXP0 + index);
        public float SpotCutoff => (Type == LightTypeEnum.PointLight) ? 180 : data.Float1(GpuOpCodes.SPOTCUT0 + index);
        public ColorfStruct AmbientColor => new ColorfStruct().SetRGB_A1(data.Int(GpuOpCodes.ALC0 + index));
        public ColorfStruct DiffuseColor => new ColorfStruct().SetRGB_A1(data.Int(GpuOpCodes.DLC0 + index));
        public ColorfStruct SpecularColor => new ColorfStruct().SetRGB_A1(data.Int(GpuOpCodes.SLC0 + index));
    }

    public class LineSmoothStateStruct
    {
        private GpuStateData data;

        public LineSmoothStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.AAE);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LogicalOperationStateStruct
    {
        private GpuStateData data;

        public LogicalOperationStateStruct(GpuStateData data) : this()
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.LOE);
        public LogicalOperationEnum Operation => (LogicalOperationEnum) data.Param8(GpuOpCodes.LOP, 0);
    }

    public class MorphingStateStruct
    {
        private GpuStateData data;

        public MorphingStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public float MorphWeight(int index) => data.Float1(GpuOpCodes.MW0 + (byte) index);
    }

    public class PatchCullingStateStruct
    {
        private GpuStateData data;

        public PatchCullingStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.PCE);
        public bool FaceFlag => (data.Param24(GpuOpCodes.PFACE) != 0);
    }

    public class ScreenBufferStateStruct
    {
        private GpuStateData data;

        public ScreenBufferStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        
        //public uint Width => data.Param16(GpuOpCodes.ZBW, 0); // @TODO: This overlaps!
        public uint Width => data.Param16(GpuOpCodes.FBW, 0);
        public GuPixelFormats Format => (GuPixelFormats) data.Param8(GpuOpCodes.PSM, 0);

        //public byte HighAddress => (byte)data.Param8(GpuOpCodes.ZBW, 16); // @TODO: This overlaps!
        //public uint LowAddress => data.Param24(GpuOpCodes.ZBP); // @TODO: This overlaps!
        
        public byte HighAddress => (byte)data.Param8(GpuOpCodes.FBW, 16);
        public uint LowAddress => data.Param24(GpuOpCodes.FBP);

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

    public class StencilStateStruct
    {
        private GpuStateData data;

        public StencilStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public bool Enabled => data.Bool(GpuOpCodes.STE);
        public TestFunctionEnum Function => (TestFunctionEnum) data.Param8(GpuOpCodes.STST, 0);
        public byte FunctionRef => (byte)data.Param8(GpuOpCodes.STST, 8);
        public byte FunctionMask => (byte)data.Param8(GpuOpCodes.STST, 16);
        public StencilOperationEnum OperationFail =>(StencilOperationEnum) data.Param8(GpuOpCodes.SOP, 0);
        public StencilOperationEnum OperationZFail => (StencilOperationEnum) data.Param8(GpuOpCodes.SOP, 8);
        public StencilOperationEnum OperationZPass =>(StencilOperationEnum) data.Param8(GpuOpCodes.SOP, 16);
    }

    public class TextureMappingStateStruct
    {
        public GpuStateData data;

        public TextureMappingStateStruct(GpuStateData data)
        {
            this.data = data;

            TextureState = new TextureStateStruct(data);
            UploadedClutState = new ClutStateStruct(data);
            ClutState = new ClutStateStruct(data);
        }

        public TextureStateStruct TextureState;
        public ClutStateStruct UploadedClutState;
        public ClutStateStruct ClutState;

        public bool Enabled => data.Bool(GpuOpCodes.TME);
        public GpuMatrix4X4Struct Matrix;
        public ColorbStruct TextureEnviromentColor => new ColorbStruct().SetRGB_A1(data.Param24(GpuOpCodes.TEC));
        public TextureMapMode TextureMapMode => (TextureMapMode) data.Param8(GpuOpCodes.TMAP, 0);
        public TextureProjectionMapMode TextureProjectionMapMode => (TextureProjectionMapMode) data.Param8(GpuOpCodes.TMAP, 8);

        public short ShadeU => (short) data.Extract(GpuOpCodes.TEXTURE_ENV_MAP_MATRIX, 0, 2);
        public short ShadeV => (short) data.Extract(GpuOpCodes.TEXTURE_ENV_MAP_MATRIX, 8, 2); 
        public TextureLevelMode LevelMode => (TextureLevelMode) data.Param8(GpuOpCodes.TBIAS, 0);
        public float MipmapBias => data.Param8(GpuOpCodes.TBIAS, 16) / 16.0f;
        public float SlopeLevel => data.Float1(GpuOpCodes.TSLOPE);

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

    public class TextureStateStruct
    {
        public GpuStateData data;

        public TextureStateStruct(GpuStateData data)
        {
            this.data = data;
            this.Mipmaps = new MipmapState[8];
            for (var n = 0; n < 8; n++) this.Mipmaps[n] = new MipmapState(data, (byte)n); 
        }

        /// <summary>
        /// MipmapState list
        /// </summary>
        public MipmapState[] Mipmaps;

        public MipmapState Mipmap0 => Mipmaps[0]; 


        public class MipmapState
        {
            private GpuStateData data;
            private byte index;

            public MipmapState(GpuStateData data, byte index)
            {
                this.data = data;
                this.index = index;
            }

            /// <summary>
            /// Pointer 
            /// </summary>
            public uint Address => (data.Int(GpuOpCodes.TBP0 + index) & 0xFFFFFF) |
                                   (data.Param8(GpuOpCodes.TBW0 + index, 16) << 24);

            /// <summary>
            /// Data width of the image.
            /// With will be bigger. For example:
            /// Bufferwidth = 480, Width = 512, Height = 512
            /// Texture is 512x512 but there is data just for 480x512
            /// </summary>
            public ushort BufferWidth => (ushort)data.Param16(GpuOpCodes.TBW0 + index, 0);

            /// <summary>
            /// Texture Width
            /// </summary>
            public ushort TextureWidth =>
                (ushort) (1 << Math.Min((int) data.Extract(GpuOpCodes.TSIZE0 + index, 0, 4), 9));


            /// <summary>
            /// Texture Height
            /// </summary>
            public ushort TextureHeight =>
                (ushort) (1 << Math.Min((int) data.Extract(GpuOpCodes.TSIZE0 + index, 8, 4), 9));
        }

        /// <summary>
        /// Is texture swizzled?
        /// </summary>
        public bool Swizzled => (data.Param8(GpuOpCodes.TMODE, 0) != 0);

        /// <summary>
        /// Mipmaps share clut?
        /// </summary>
        public bool MipmapShareClut => (data.Param8(GpuOpCodes.TMODE, 8) != 0);

        /// <summary>
        /// Levels of mipmaps
        /// </summary>
        public int MipmapMaxLevel => (int)data.Param8(GpuOpCodes.TMODE, 16);

        /// <summary>
        /// Format of the texture data.
        /// Texture Data mode
        /// </summary>
        public GuPixelFormats PixelFormat => (GuPixelFormats) data.Extract(GpuOpCodes.TPSM, 0, 4);

        /// <summary>
        /// TextureFilter when drawing the texture scaled
        /// </summary>
        public TextureFilter FilterMinification => (TextureFilter) data.Param8(GpuOpCodes.TFLT, 0);

        /// <summary>
        /// TextureFilter when drawing the texture scaled
        /// </summary>
        public TextureFilter FilterMagnification => (TextureFilter) data.Param8(GpuOpCodes.TFLT, 8);

        /// <summary>
        /// Wrap mode when specifying texture coordinates beyond texture size
        /// </summary>
        public WrapMode WrapU => (WrapMode) data.Param8(GpuOpCodes.TWRAP, 0);

        /// <summary>
        /// Wrap mode when specifying texture coordinates beyond texture size
        /// </summary>
        public WrapMode WrapV => (WrapMode) data.Param8(GpuOpCodes.TWRAP, 8);

        public float ScaleU => data.Float1(GpuOpCodes.USCALE);
        public float ScaleV => data.Float1(GpuOpCodes.VSCALE);
        public float OffsetU => data.Float1(GpuOpCodes.UOFFSET);
        public float OffsetV => data.Float1(GpuOpCodes.VOFFSET);

        public bool Fragment2X => (data.Param8(GpuOpCodes.TFUNC, 16) != 0);
        public TextureEffect Effect => (TextureEffect) data.Param8(GpuOpCodes.TFUNC, 0);
        public TextureColorComponent ColorComponent => (TextureColorComponent) data.Param8(GpuOpCodes.TFUNC, 8);
    }

    public class TextureTransferStateStruct
    {
        public GpuStateData data;

        public TextureTransferStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        public enum TexelSizeEnum : ushort
        {
            Bit16 = 0,
            Bit32 = 1
        }

        public int BytesPerPixel => (TexelSize == TexelSizeEnum.Bit16) ? 2 : 4;
        public int SourceLineWidthInBytes => SourceLineWidth * BytesPerPixel;
        public int DestinationLineWidthInBytes => DestinationLineWidth * BytesPerPixel;
        public int WidthInBytes => Width * BytesPerPixel;

        public PspPointer SourceAddress => data.Extract(GpuOpCodes.TRXSBP, 0, 24) | (data.Extract(GpuOpCodes.TRXSBW, 16, 8) << 24);
        public PspPointer DestinationAddress => data.Extract(GpuOpCodes.TRXDBP, 0, 24) | (data.Extract(GpuOpCodes.TRXDBW, 16, 8) << 24);
        public ushort SourceLineWidth => (ushort) data.Extract(GpuOpCodes.TRXSBW, 0, 16);
        public ushort DestinationLineWidth => (ushort) data.Extract(GpuOpCodes.TRXDBW, 0, 16);
        public ushort SourceX => (ushort) data.Extract(GpuOpCodes.TRXSPOS, 10 * 0, 10);
        public ushort SourceY => (ushort) data.Extract(GpuOpCodes.TRXSPOS, 10 * 1, 10);
        public ushort DestinationX => (ushort) data.Extract(GpuOpCodes.TRXDPOS, 10 * 0, 10);
        public ushort DestinationY => (ushort) data.Extract(GpuOpCodes.TRXDPOS, 10 * 1, 10);
        public ushort Width => (ushort) (data.Extract(GpuOpCodes.TRXSIZE, 10 * 0, 10) + 1);
        public ushort Height => (ushort) (data.Extract(GpuOpCodes.TRXSIZE, 10 * 1, 10) + 1);
        
        public TexelSizeEnum TexelSize;
    }

    public class VertexTypeStruct
    {
        private GpuStateData data;

        public VertexTypeStruct(GpuStateData data)
        {
            this.data = data;
        }

        public bool Equals(VertexTypeStruct other) => ReversedNormal == other.ReversedNormal &&
                                                      NormalCount == other.NormalCount && Value == other.Value;

        public bool ReversedNormal => data.Bool(GpuOpCodes.RNORM);
        public byte NormalCount; // => GpuState.TextureMappingState.GetTextureComponentsCount()
        public uint Value
        {
            get { return data[GpuOpCodes.VTYPE]; }
            set { data[GpuOpCodes.VTYPE] = value; }
        }

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
            set => Value = BitUtils.Insert(Value, 9, 2, (uint) value);
        }

        public bool HasTexture => Texture != NumericEnum.Void;

        public NumericEnum Texture
        {
            get => (NumericEnum) BitUtils.Extract(Value, 0, 2);
            set => Value = BitUtils.Insert(Value, 0, 2, (uint) value);
        }

        public bool HasColor => Color != ColorEnum.Void;

        public ColorEnum Color
        {
            get => (ColorEnum) BitUtils.Extract(Value, 2, 3);
            set => Value = BitUtils.Insert(Value, 2, 3, (uint) value);
        }

        public bool HasNormal => Normal != NumericEnum.Void;

        public NumericEnum Normal
        {
            get => (NumericEnum) BitUtils.Extract(Value, 5, 2);
            set => Value = BitUtils.Insert(Value, 5, 2, (uint) value);
        }

        public bool HasPosition => Position != NumericEnum.Void;

        public NumericEnum Position
        {
            get => (NumericEnum) BitUtils.Extract(Value, 7, 2);
            set => Value = BitUtils.Insert(Value, 7, 2, (uint) value);
        }

        public bool HasIndex => Index != IndexEnum.Void;

        public IndexEnum Index
        {
            get => (IndexEnum) BitUtils.Extract(Value, 11, 2);
            set => Value = BitUtils.Insert(Value, 11, 2, (uint) value);
        }

        public int SkinningWeightCount
        {
            get => (int) BitUtils.Extract(Value, 14, 3);
            set => Value = BitUtils.Insert(Value, 14, 3, (uint) value);
        }

        public int MorphingVertexCount
        {
            get => (int) BitUtils.Extract(Value, 18, 2);
            set => Value = BitUtils.Insert(Value, 18, 2, (uint) value);
        }

        public bool Transform2D
        {
            get => BitUtils.Extract(Value, 23, 1) != 0;
            set => Value = BitUtils.Insert(Value, 23, 1, value ? 1U : 0U);
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

    public class VertexStateStruct
    {
        private GpuStateData data;

        public VertexStateStruct(GpuStateData data)
        {
            this.data = data;
        }

        
        public GpuMatrix4X4Struct ProjectionMatrix => new GpuMatrix4X4Struct().ResetAndWriteAll(SpanExt.Reinterpret<float, uint>(data.Span.Slice((int)GpuOpCodes.PROJ_MATRIX_BASE)));
        public GpuMatrix4X3Struct WorldMatrix => new GpuMatrix4X3Struct().ResetAndWriteAll(SpanExt.Reinterpret<float, uint>(data.Span.Slice((int)GpuOpCodes.WORLD_MATRIX_BASE)));
        public GpuMatrix4X3Struct ViewMatrix => new GpuMatrix4X3Struct().ResetAndWriteAll(SpanExt.Reinterpret<float, uint>(data.Span.Slice((int)GpuOpCodes.VIEW_MATRIX_BASE)));
        public TransformModeEnum TransformMode;
        public VertexTypeStruct Type => new VertexTypeStruct(data);
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

        public override string ToString() => this.ToStringDefault();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PointS
    {
        public short X, Y;

        public PointS(short x, short y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => this.ToStringDefault();
    }

    public class ViewportStruct
    {
        private GpuStateData data;

        public ViewportStruct(GpuStateData data)
        {
            this.data = data;
        }

        public Vector4 Position => new Vector4(data.Float1(GpuOpCodes.XPOS), data.Float1(GpuOpCodes.YPOS), BitUtils.ExtractUnsignedScaled(data.Int(GpuOpCodes.ZPOS), 0, 16, 1.0f), 1f);
        public Vector4 Scale => new Vector4(data.Float1(GpuOpCodes.XSCALE) * 2, data.Float1(GpuOpCodes.YSCALE) * -2, data.Float1(GpuOpCodes.ZSCALE), 1f);
        public PointS RegionTopLeft => new PointS((short) data.Extract(GpuOpCodes.REGION1, 0, 10), (short) data.Extract(GpuOpCodes.REGION1, 10, 10));
        public PointS RegionBottomRight => new PointS((short) (data.Extract(GpuOpCodes.REGION2, 0, 10) + 1), (short) (data.Extract(GpuOpCodes.REGION2, 10, 10) + 1));
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

        public ColorbStruct SetRGB_A1(uint params24)
        {
            Alpha = 0xFF;
            
            return this;
        }

        public ColorbStruct SetRgb(uint params24)
        {
            Red = (byte) ((params24 >> 0) & 0xFF);
            Green = (byte) ((params24 >> 8) & 0xFF);
            Blue = (byte) ((params24 >> 16) & 0xFF);
            return this;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorfStruct
    {
        public float Red, Green, Blue, Alpha;

        public ColorfStruct SetRGB_A1(uint params24)
        {
            SetRgb(params24);
            Alpha = 1.0f;
            return this;
        }

        public ColorfStruct SetRgb(uint params24)
        {
            //Console.WriteLine(Params24);
            Red = ((params24 >> 0) & 0xFF) / 255.0f;
            Green = ((params24 >> 8) & 0xFF) / 255.0f;
            Blue = ((params24 >> 16) & 0xFF) / 255.0f;
            return this;
        }

        public ColorfStruct SetA(uint params24)
        {
            Alpha = ((params24 >> 0) & 0xFF) / 255.0f;
            return this;
        }

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