using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State.SubStates;

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

	public struct Vector3f
	{
		public float X, Y, Z;
	}

	public struct ViewportStruct
	{
		public Vector3f Position;
		public Vector3f Scale;
	}

	public struct ColorfStruct
	{
		public float Red, Green, Blue, Alpha;

		public void SetRGB(uint Params24)
		{
			Red = ((float)((Params24 >> 0) & 0xFF)) / 255.0f;
			Green = ((float)((Params24 >> 8) & 0xFF)) / 255.0f;
			Blue = ((float)((Params24 >> 16) & 0xFF)) / 255.0f;
			Alpha = 1.0f;
		}
	}

	public enum LogicalOperationEnum
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

	public enum StencilOperationEnum {
		Keep = 0,
		Zero = 1,
		Replace = 2,
		Invert = 3,
		Increment = 4,
		Decrement = 5,
	}

	public enum ShadingModelEnum
	{
		Flat = 0,
		Smooth = 1,
	}

	public enum ClearBufferSet
	{
		ColorBuffer = 1,
		StencilBuffer = 2,
		DepthBuffer = 4,
		FastClear = 16
	}

	public struct ScreenBufferStateStruct {
		public uint Width;
		public PspDisplay.PixelFormats Format;

		public byte HighAddress;
		public uint LowAddress;

		public uint LoadAddress;
		public uint StoreAddress;

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

	public struct GpuStateStruct
	{
		public uint BaseAddress;
		public uint VertexAddress;
		public uint IndexAddress;
		public ScreenBufferStateStruct DrawBufferState;
		public ScreenBufferStateStruct DepthBufferState;
		public TextureTransferStateStruct TextureTransferState;

		public ViewportStruct Viewport;
		public PointI Offset;
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

		// State.
		public ColorfStruct fixColorSrc, fixColorDst;

		public TextureMappingStateStruct TextureMappingState;
	}
}
