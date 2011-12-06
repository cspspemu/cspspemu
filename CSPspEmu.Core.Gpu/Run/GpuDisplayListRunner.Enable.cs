using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
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

		// Backface Culling Enable (GU_CULL_FACE)
		public void OP_BCE()
		{
			GpuState[0].BackfaceCullingState.Enabled = Bool1;
		}

		// DiThering Enable (GU_DITHER)
		public void OP_DTE()
		{
			GpuState[0].DitheringState.Enabled = Bool1;
		}

		// Clip Plane Enable (GU_CLIP_PLANES/GL_CLIP_PLANE0)
		public void OP_CPE()
		{
			GpuState[0].ClipPlaneState.Enabled = Bool1;
		}

		// AnitAliasing Enable (GU_LINE_SMOOTH?)
		public void OP_AAE()
		{
			GpuState[0].LineSmoothState.Enabled = Bool1;
		}

		// Patch Cull Enable (GU_PATCH_CULL_FACE)
		public void OP_PCE()
		{
			GpuState[0].PatchCullingState.Enabled = Bool1;
		}

		// Color Test Enable (GU_COLOR_TEST)
		public void OP_CTE()
		{
			GpuState[0].ColorTestState.Enabled = Bool1;
		}
	
	}
}
