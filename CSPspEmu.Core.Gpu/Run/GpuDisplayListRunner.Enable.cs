namespace CSPspEmu.Core.Gpu.Run
{
	public unsafe sealed partial class GpuDisplayListRunner
	{
		/**
		 * Enable GE state
		 *
		 * The currently available states are:
		 *   - GU_ALPHA_TEST
		 *   - GU_DEPTH_TEST
		 *   - GU_SCISSOR_TEST
		 *   - GU_STENCIL_TEST
		 *   - GU_BLEND
		 *   - GU_CULL_FACE
		 *   - GU_DITHER
		 *   - GU_FOG
		 *   - GU_CLIP_PLANES
		 *   - GU_TEXTURE_2D
		 *   - GU_LIGHTING
		 *   - GU_LIGHT0
		 *   - GU_LIGHT1
		 *   - GU_LIGHT2
		 *   - GU_LIGHT3
		 *   - GU_LINE_SMOOTH
		 *   - GU_PATCH_CULL_FACE
		 *   - GU_COLOR_TEST
		 *   - GU_COLOR_LOGIC_OP
		 *   - GU_FACE_NORMAL_REVERSE
		 *   - GU_PATCH_FACE
		 *   - GU_FRAGMENT_2X
		 *
		 * @param state - Which state to enable
		 **/
		// void sceGuEnable(int state);

		// (GU_SCISSOR_TEST) // OP_SCISSOR1 + OP_SCISSOR2

		/// <summary>
		/// Backface Culling Enable (GU_CULL_FACE)
		/// </summary>
		public void OP_BCE()
		{
			GpuState->BackfaceCullingState.Enabled = Bool1;
		}

		/// <summary>
		/// DiThering Enable (GU_DITHER)
		/// </summary>
		public void OP_DTE()
		{
			GpuState->DitheringState.Enabled = Bool1;
		}

		/// <summary>
		/// Clip Plane Enable (GU_CLIP_PLANES/GL_CLIP_PLANE0)
		/// </summary>
		public void OP_CPE()
		{
			GpuState->ClipPlaneState.Enabled = Bool1;
		}

		/// <summary>
		/// AnitAliasing Enable (GU_LINE_SMOOTH?)
		/// </summary>
		public void OP_AAE()
		{
			GpuState->LineSmoothState.Enabled = Bool1;
		}

		/// <summary>
		/// Patch Cull Enable (GU_PATCH_CULL_FACE)
		/// </summary>
		public void OP_PCE()
		{
			GpuState->PatchCullingState.Enabled = Bool1;
		}

		/// <summary>
		/// Color Test Enable (GU_COLOR_TEST)
		/// </summary>
		public void OP_CTE()
		{
			GpuState->ColorTestState.Enabled = Bool1;
		}
	
	}
}
