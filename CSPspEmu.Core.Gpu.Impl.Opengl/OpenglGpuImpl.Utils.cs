#if OPENTK
using OpenTK.Graphics.OpenGL;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public sealed partial class OpenglGpuImpl
	{
		private static bool GlEnableDisable(EnableCap EnableCap, bool EnableDisable)
		{
			if (EnableDisable)
			{
				GL.Enable(EnableCap);
			}
			else
			{
				GL.Disable(EnableCap);
			}
			return EnableDisable;
		}
	}
}
