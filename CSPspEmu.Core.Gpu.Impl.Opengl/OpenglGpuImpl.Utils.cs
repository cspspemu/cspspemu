using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public partial class OpenglGpuImpl
	{
		static private bool GlEnableDisable(EnableCap EnableCap, bool EnableDisable)
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
