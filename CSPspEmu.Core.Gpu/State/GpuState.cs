using CSPspEmu.Core.Gpu.State.SubStates;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State
{
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
		public int BaseOffset;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct GpuStateStruct
	{
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
}
