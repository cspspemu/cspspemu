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
		[GpuOpCodesNotImplemented]
		public void OP_BCE()
		{
			//gpu.state.backfaceCullingEnabled = command.bool1;
		}

		// DiThering Enable (GU_DITHER)
		[GpuOpCodesNotImplemented]
		public void OP_DTE()
		{
			//gpu.state.ditheringEnabled = command.bool1;
		}

		// Clip Plane Enable (GU_CLIP_PLANES/GL_CLIP_PLANE0)
		[GpuOpCodesNotImplemented]
		public void OP_CPE()
		{
			//gpu.state.clipPlaneEnabled = command.bool1;
		}

		// AnitAliasing Enable (GU_LINE_SMOOTH?)
		[GpuOpCodesNotImplemented]
		public void OP_AAE()
		{
			//gpu.state.lineSmoothEnabled = command.bool1;
		}

		// Patch Cull Enable (GU_PATCH_CULL_FACE)
		[GpuOpCodesNotImplemented]
		public void OP_PCE()
		{
			//gpu.state.patchCullEnabled = command.bool1;
		}

		// Color Test Enable (GU_COLOR_TEST)
		[GpuOpCodesNotImplemented]
		public void OP_CTE()
		{
			//gpu.state.colorTestEnabled = command.bool1;
		}
	
	}
}
