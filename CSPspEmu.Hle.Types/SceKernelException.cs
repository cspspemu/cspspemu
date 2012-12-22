using System;

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
