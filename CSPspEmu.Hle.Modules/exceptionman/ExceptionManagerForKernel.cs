using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.exceptionman
{
	public class ExceptionManagerForKernel : HleModuleHost
	{
		/// <summary>
		/// Register a default exception handler.
		/// 
		/// NOTE: The exception handler function must start with a NOP
		/// </summary>
		/// <param name="ExceptionHandlerFunction">Pointer to the exception handler function</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x565C0B0E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelRegisterDefaultExceptionHandler(uint ExceptionHandlerFunction)
		{
			/*
			if (ExceptionHandlerFunction is null) return -1;
			exceptionHandler = ExceptionHandlerFunction;
			//unimplemented();
			return 0;
			*/
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
