using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSPspEmu.Core.Display;

namespace CSPspEmu.Core.Gpu.State
{
	public class GpuState
	{
		public GpuStateStruct Value;
	}

	public struct PointI
	{
		public uint X, Y;
	}

	public struct PointS
	{
		public short X, Y;
	}

	public struct Vector3f
	{
		public float X, Y, Z;
	}

	public struct ViewportStruct
	{
		public Vector3f Position;
		public Vector3f Scale;
		public PointS RegionTopLeft;
		public PointS RegionBottomRight;
	}

	public struct ColorfStruct
	{
		public float Red, Green, Blue, Alpha;

		public void SetRGB_A1(uint Params24)
		{
			SetRGB(Params24);
			Alpha = 1.0f;
		}

		public void SetRGB(uint Params24)
		{
			//Console.WriteLine(Params24);
			Red = ((float)((Params24 >> 0) & 0xFF)) / 255.0f;
			Green = ((float)((Params24 >> 8) & 0xFF)) / 255.0f;
			Blue = ((float)((Params24 >> 16) & 0xFF)) / 255.0f;
		}

		public void SetA(uint Params24)
		{
			Alpha = ((float)((Params24 >> 0) & 0xFF)) / 255.0f;
		}

		public override string ToString()
		{
			return String.Format("Colorf(R={0}, G={1}, B={2}, A={3})", Red, Green, Blue, Alpha);
		}

		public bool IsColorf(float R, float G, float B)
		{
			return R == this.Red && G == this.Green && B == this.Blue;
		}

		public static ColorfStruct operator +(ColorfStruct Left, ColorfStruct Right)
		{
			return new ColorfStruct()
			{
				Red = Left.Red + Right.Red,
				Green = Left.Green + Right.Green,
				Blue = Left.Blue + Right.Blue,
				Alpha = Left.Alpha + Right.Alpha,
			};
		}

	}

	public enum PrimitiveType : byte
	{
		Points = 0,
		Lines = 1,
		LineStrip = 2,
		Triangles = 3,
		TriangleStrip = 4,
		TriangleFan = 5,
		Sprites = 6,
	}

	public enum TextureColorComponent
	{
		Rgb = 0,
		Rgba = 1,
	}

	public enum TextureEffect
	{
		Modulate = 0,
		Decal = 1,
		Blend = 2,
		Replace = 3,
		Add = 4,
	}

	public enum TextureFilter
	{
		Nearest = 0,
		Linear = 1,

		NearestMipmapNearest = 4,
		LinearMipmapNearest = 5,
		NearestMipmapLinear = 6,
		LinearMipmapLinear = 7,
	}

	public enum WrapMode
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

	public enum ClearBufferSet : byte
	{
		ColorBuffer = 1,
		StencilBuffer = 2,
		DepthBuffer = 4,
		FastClear = 16
	}

	public struct ScreenBufferStateStruct {
		public uint Width;
		public GuPixelFormats Format;

		public byte HighAddress;
		public uint LowAddress;

		public uint LoadAddress;
		public uint StoreAddress;

		public uint Address
		{
			get
			{
				return 0x04000000 | ((uint)HighAddress << 24) | LowAddress;
			}
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
	}

	public struct TextureTransferStateStruct
	{
		public enum TexelSize : ushort { BIT_16 = 0, BIT_32 = 1 }
		//enum TexelSize { BIT_32 = 0, BIT_16 = 1 }

		public uint srcAddress, dstAddress;
		public ushort srcLineWidth, dstLineWidth;
		public ushort srcX, srcY, dstX, dstY;
		public ushort width, height;
		public TexelSize texelSize;
	}

	public enum BlendingOpEnum : int
	{
		Add = 0,
		Substract = 1,
		ReverseSubstract = 2,
		Min = 3,
		Max = 4,
		Abs = 5,
	}

	public enum GuBlendingFactorSource : int
	{
		// Source
		GU_SRC_COLOR = 0, GU_ONE_MINUS_SRC_COLOR = 1, GU_SRC_ALPHA = 2, GU_ONE_MINUS_SRC_ALPHA = 3,
		// Both?
		GU_FIX = 10
	}

	public enum GuBlendingFactorDestination : int
	{
		// Dest
		GU_DST_COLOR = 0, GU_ONE_MINUS_DST_COLOR = 1, GU_DST_ALPHA = 4, GU_ONE_MINUS_DST_ALPHA = 5,
		// Both?
		GU_FIX = 10
	}

	public struct GpuStateStruct
	{
		public uint BaseAddress;
		public uint VertexAddress;
		public uint IndexAddress;
		public ScreenBufferStateStruct DrawBufferState;
		public ScreenBufferStateStruct DepthBufferState;
		public TextureTransferStateStruct TextureTransferState;

		public ViewportStruct Viewport;
		public PointS Offset;
		public bool ToggleUpdateState;

		/// <summary>
		/// A set of flags related to the clearing mode. Generally which buffers to clear.
		/// </summary>
		public ClearBufferSet ClearFlags;

		/// <summary>
		/// When set, this will changes the Draw behaviour.
		/// </summary>
		public bool ClearingMode;


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

		// State.
		public ColorfStruct FixColorSource, FixColorDestination;

		public TextureMappingStateStruct TextureMappingState;

		/// <summary>
		/// 
		/// </summary>
		public ShadingModelEnum ShadeModel;
	}
}
