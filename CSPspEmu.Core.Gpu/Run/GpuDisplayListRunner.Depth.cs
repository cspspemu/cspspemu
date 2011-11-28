using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		/**
		 * Set depth buffer parameters
		 *
		 * @param zbp - VRAM pointer where the depthbuffer should start
		 * @param zbw - The width of the depth-buffer (block-aligned)
		 *
		 **/
		// void sceGuDepthBuffer(void* zbp, int zbw);

		// Depth Buffer Pointer
		public void OP_ZBP()
		{
			GpuState[0].DepthBufferState.LowAddress = Params24;
		}

		// Depth Buffer Width
		public void OP_ZBW()
		{
			GpuState[0].DepthBufferState.HighAddress = Param8(16);
			GpuState[0].DepthBufferState.Width = Param16(0);
			GpuDisplayList.GpuProcessor.MarkDepthBufferLoad();
		}

		// depth (Z) Test Enable (GU_DEPTH_TEST)
		public void OP_ZTE()
		{
			GpuState[0].DepthTestState.Enabled = Bool1;
		}

		/**
		 * Select which depth-test function to use
		 *
		 * Valid choices for the depth-test are:
		 *   - GU_NEVER - No pixels pass the depth-test
		 *   - GU_ALWAYS - All pixels pass the depth-test
		 *   - GU_EQUAL - Pixels that match the depth-test pass
		 *   - GU_NOTEQUAL - Pixels that doesn't match the depth-test pass
		 *   - GU_LESS - Pixels that are less in depth passes
		 *   - GU_LEQUAL - Pixels that are less or equal in depth passes
		 *   - GU_GREATER - Pixels that are greater in depth passes
		 *   - GU_GEQUAL - Pixels that are greater or equal passes
		 *
		 * @param function - Depth test function to use
		 **/
		// void sceGuDepthFunc(int function); // OP_ZTST
		public void OP_ZTST()
		{
			GpuState[0].DepthTestState.Function = (TestFunctionEnum)Param8(0);
		}

		// Alpha Test Enable (GU_ALPHA_TEST) glAlphaFunc(GL_GREATER, 0.03f);
		public void OP_ATE()
		{
			GpuState[0].AlphaTestState.Enabled = Bool1;
		}

		/**
		 * Set the alpha test parameters
		 * 
		 * Available comparison functions are:
		 *   - GU_NEVER
		 *   - GU_ALWAYS
		 *   - GU_EQUAL
		 *   - GU_NOTEQUAL
		 *   - GU_LESS
		 *   - GU_LEQUAL
		 *   - GU_GREATER
		 *   - GU_GEQUAL
		 *
		 * @param func - Specifies the alpha comparison function.
		 * @param value - Specifies the reference value that incoming alpha values are compared to.
		 * @param mask - Specifies the mask that both values are ANDed with before comparison.
		 **/
		// void sceGuAlphaFunc(int func, int value, int mask); // OP_ATST
		public void OP_ATST()
		{
			GpuState[0].AlphaTestState.Function = (TestFunctionEnum)Param8(0);
			GpuState[0].AlphaTestState.Value = (float)Param8(8) / 255.0f;
			GpuState[0].AlphaTestState.Mask = Param8(16);
		}

		// Stencil Test Enable (GL_STENCIL_TEST)
		public void OP_STE()
		{
			GpuState[0].StencilState.Enabled = Bool1;
		}

		/**
		 * Set stencil function and reference value for stencil testing
		 *
		 * Available functions are:
		 *   - GU_NEVER
		 *   - GU_ALWAYS
		 *   - GU_EQUAL
		 *   - GU_NOTEQUAL
		 *   - GU_LESS
		 *   - GU_LEQUAL
		 *   - GU_GREATER
		 *   - GU_GEQUAL
		 *
		 * @param func - Test function
		 * @param ref - The reference value for the stencil test
		 * @param mask - Mask that is ANDed with both the reference value and stored stencil value when the test is done
		 **/
		// void sceGuStencilFunc(int func, int ref, int mask); // OP_STST
		// sendCommandi(220,func | ((ref & 0xff) << 8) | ((mask & 0xff) << 16));
		// Stencil Test
		public void OP_STST()
		{
			GpuState[0].StencilState.Function = (TestFunctionEnum)Param8(0);
			GpuState[0].StencilState.FunctionRef = Param8(8);
			GpuState[0].StencilState.FunctionMask = Param8(16);
		}

		/**
		 * Set the stencil test actions
		 *
		 * Available actions are:
		 *   - GU_KEEP - Keeps the current value
		 *   - GU_ZERO - Sets the stencil buffer value to zero
		 *   - GU_REPLACE - Sets the stencil buffer value to ref, as specified by sceGuStencilFunc()
		 *   - GU_INCR - Increments the current stencil buffer value
		 *   - GU_DECR - Decrease the current stencil buffer value
		 *   - GU_INVERT - Bitwise invert the current stencil buffer value
		 *
		 * As stencil buffer shares memory with framebuffer alpha, resolution of the buffer
		 * is directly in relation.
		 *
		 * @param fail - The action to take when the stencil test fails
		 * @param zfail - The action to take when stencil test passes, but the depth test fails
		 * @param zpass - The action to take when both stencil test and depth test passes
		 **/
		// void sceGuStencilOp(int fail, int zfail, int zpass); // OP_SOP

		// Stencil OPeration
		public void OP_SOP()
		{
			GpuState[0].StencilState.OperationSFail = (StencilOperationEnum)Param8(0);
			GpuState[0].StencilState.OperationDpFail = (StencilOperationEnum)Param8(8);
			GpuState[0].StencilState.OperationDpPass = (StencilOperationEnum)Param8(16);
		}

		/**
		 * Mask depth buffer writes
		 *
		 * @param mask - GU_TRUE(1) to disable Z writes, GU_FALSE(0) to enable
		 **/
		// void sceGuDepthMask(int mask);

		// glDepthMask
		public void OP_ZMSK()
		{
			GpuState[0].DepthTestState.Mask = Param16(0);
		}

		/**
		 * Set which range to use for depth calculations.
		 *
		 * @note The depth buffer is inversed, and takes values from 65535 to 0.
		 *
		 * Example: Use the entire depth-range for calculations:
		 * @code
		 * sceGuDepthRange(65535,0);
		 * @endcode
		 *
		 * @param near - Value to use for the near plane
		 * @param far - Value to use for the far plane
		 **/
		// void sceGuDepthRange(int near, int far); // OP_NEARZ + OP_FARZ
		// void sceGuDepthOffset(unsigned int offset);
		public void OP_NEARZ()
		{
			GpuState[0].DepthTestState.RangeNear = ((float)(short)Param16(0)) / 65535.0f;
		}
		public void OP_FARZ()
		{
			GpuState[0].DepthTestState.RangeFar = ((float)(short)Param16(0)) / 65535.0f;
		}
	}
}
