using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle
{
	public class SceKernelException : Exception
	{
		public readonly SceKernelErrors SceKernelError;

		public SceKernelException(SceKernelErrors SceKernelError, string Message = "")
			: base(SceKernelError + " : " + Message)
		{
			this.SceKernelError = SceKernelError;
		}
	}
}
