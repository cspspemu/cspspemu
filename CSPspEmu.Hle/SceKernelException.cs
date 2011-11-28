using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	public class SceKernelException : Exception
	{
		public readonly SceKernelErrors SceKernelError;

		public SceKernelException(SceKernelErrors SceKernelError)
		{
			this.SceKernelError = SceKernelError;
		}
	}
}
