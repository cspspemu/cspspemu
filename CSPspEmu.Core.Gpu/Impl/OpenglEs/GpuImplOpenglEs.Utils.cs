using GLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
	public sealed unsafe partial class GpuImplOpenglEs
	{
		private static bool GlEnableDisable(int EnableCap, bool EnableDisable)
		{
			if (EnableDisable)
			{
				GL.glEnable(EnableCap);
			}
			else
			{
				GL.glDisable(EnableCap);
			}
			return EnableDisable;
		}
	}
}
