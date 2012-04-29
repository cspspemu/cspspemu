using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State
{
	public enum TransformModeEnum
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

	[Flags]
	public enum ClearBufferSet : byte
	{
		ColorBuffer = 1,
		StencilBuffer = 2,
		DepthBuffer = 4,
		FastClear = 16
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
}
