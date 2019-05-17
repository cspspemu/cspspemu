using CSharpPlatform.GL;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Modules
{
    internal unsafe class OpenglGpuImplClear
    {
        internal static void PrepareStateClear(GpuStateStruct gpuState)
        {
            bool ccolorMask = false, calphaMask = false;

            //return;

            GL.glDisable(GL.GL_BLEND);
            //GL.glDisable(GL.GL_LIGHTING);
            GL.glDisable(GL.GL_TEXTURE_2D);
            //GL.glDisable(GL.GL_ALPHA_TEST);
            GL.glDisable(GL.GL_DEPTH_TEST);
            GL.glDisable(GL.GL_STENCIL_TEST);
            //GL.glDisable(GL.GL_FOG);
            //GL.glDisable(GL.GL_LOGIC_OP);
            GL.glDisable(GL.GL_CULL_FACE);
            GL.glDepthMask(false);

            if (gpuState.ClearFlags.HasFlag(ClearBufferSet.ColorBuffer))
            {
                ccolorMask = true;
            }

            if (GL.EnableDisable(GL.GL_STENCIL_TEST, gpuState.ClearFlags.HasFlag(ClearBufferSet.StencilBuffer)))
            {
                calphaMask = true;
                // Sets to 0x00 the stencil.
                // @TODO @FIXME! : Color should be extracted from the color! (as alpha component)
                GL.glStencilFunc(GL.GL_ALWAYS, 0x00, 0xFF);
                GL.glStencilOp(GL.GL_REPLACE, GL.GL_REPLACE, GL.GL_REPLACE);
                //Console.Error.WriteLine("Stencil!");
                //GL.Enable(EnableCap.DepthTest);
            }

            //int i; glGetIntegerv(GL_STENCIL_BITS, &i); writefln("GL_STENCIL_BITS: %d", i);

            if (gpuState.ClearFlags.HasFlag(ClearBufferSet.DepthBuffer))
            {
                GL.glEnable(GL.GL_DEPTH_TEST);
                GL.glDepthFunc(GL.GL_ALWAYS);
                GL.glDepthMask(true);

                GL.DepthRange(0, 0);

                //glDepthRange(0.0, 1.0); // Original value
            }

            GL.glColorMask(ccolorMask, ccolorMask, ccolorMask, calphaMask);

            //glClearDepth(0.0); glClear(GL_COLOR_BUFFER_BIT);

            //if (state.clearFlags & ClearBufferMask.GU_COLOR_BUFFER_BIT) glClear(GL_DEPTH_BUFFER_BIT);
            //GL.Clear(ClearBufferMask.StencilBufferBit);
        }
    }
}