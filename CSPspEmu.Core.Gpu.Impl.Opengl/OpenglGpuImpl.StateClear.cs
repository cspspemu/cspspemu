using CSPspEmu.Core.Gpu.State;

#if OPENTK
using OpenTK.Graphics.OpenGL;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public sealed unsafe partial class OpenglGpuImpl
	{
		void PrepareStateClear(GpuStateStruct* GpuState)
		{
			bool ccolorMask = false, calphaMask = false;

			//return;

			GL.Disable(EnableCap.Blend);
			GL.Disable(EnableCap.Lighting);
			GL.Disable(EnableCap.Texture2D);
			GL.Disable(EnableCap.AlphaTest);
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.StencilTest);
			GL.Disable(EnableCap.Fog);
			GL.Disable(EnableCap.ColorLogicOp);
			GL.Disable(EnableCap.CullFace);
			GL.DepthMask(false);

			if (GpuState->ClearFlags.HasFlag(ClearBufferSet.ColorBuffer))
			{
				ccolorMask = true;
			}

			if (GlEnableDisable(EnableCap.StencilTest, GpuState->ClearFlags.HasFlag(ClearBufferSet.StencilBuffer)))
			{
				calphaMask = true;
				// Sets to 0x00 the stencil.
				// @TODO @FIXME! : Color should be extracted from the color! (as alpha component)
				GL.StencilFunc(StencilFunction.Always, 0x00, 0xFF);
				GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
				//Console.Error.WriteLine("Stencil!");
				//GL.Enable(EnableCap.DepthTest);
			}

			//int i; glGetIntegerv(GL_STENCIL_BITS, &i); writefln("GL_STENCIL_BITS: %d", i);

			if (GpuState->ClearFlags.HasFlag(ClearBufferSet.DepthBuffer))
			{
				GL.Enable(EnableCap.DepthTest);
				GL.DepthFunc(DepthFunction.Always);
				GL.DepthMask(true);
				GL.DepthRange((double)0, (double)0);
				//GL.DepthRange((double)-1, (double)0);

				//glDepthRange(0.0, 1.0); // Original value
			}

			GL.ColorMask(ccolorMask, ccolorMask, ccolorMask, calphaMask);

			//glClearDepth(0.0); glClear(GL_COLOR_BUFFER_BIT);

			//if (state.clearFlags & ClearBufferMask.GU_COLOR_BUFFER_BIT) glClear(GL_DEPTH_BUFFER_BIT);
			//GL.Clear(ClearBufferMask.StencilBufferBit);
		}
	}
}
