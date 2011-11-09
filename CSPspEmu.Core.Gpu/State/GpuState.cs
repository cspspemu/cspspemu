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

	public struct ViewportStruct
	{
		public float PX, PY, PZ;
		public float SX, SY, SZ;
	}

	public struct ColorfStruct
	{
		public float Red, Green, Blue, Alpha;
	}

	public struct LightingState
	{
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

	public struct GpuStateStruct
	{
		public ViewportStruct Viewport;
		public PointI Offset;
		public bool ToggleUpdateState;

		public ClearBufferSet ClearFlags;
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
		public TextureMappingStateStruct TextureMappingState;
		public DitheringStateStruct DitheringState;
		public LineSmoothStateStruct LineSmoothState;
		public ClipPlaneStateStruct ClipPlaneState;
		public PatchCullingStateStruct PatchCullingState;

		// State.
		public ColorfStruct fixColorSrc, fixColorDst;
	}
}
