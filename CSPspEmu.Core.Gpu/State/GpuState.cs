using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Memory;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GpuStateStruct
	{
		public uint GetAddressRelativeOffset(uint Offset)
		{
			return (uint)((this.BaseAddress | Offset) + this.BaseOffset);
		}

		public uint BaseAddress;
		public int BaseOffset;
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

		// State.
		public ColorStruct FixColorSource, FixColorDestination;

		public TextureMappingStateStruct TextureMappingState;

		/// <summary>
		/// 
		/// </summary>
		public ShadingModelEnum ShadeModel;
	}
}
