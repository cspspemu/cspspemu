using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu
{
	public class _DynarecConfig
	{
		//public const bool UpdatePCEveryInstruction = true;
		public const bool UpdatePCEveryInstruction = false;

		public const bool FunctionCallWithStaticReferences = true;
		//public const bool FunctionCallWithStaticReferences = false;

		public const bool EnableFastPspMemoryUtilsGetFastMemoryReader = false;
		//public const bool EnableFastPspMemoryUtilsGetFastMemoryReader = true;

		public const bool AllowFastMemory = true;
		
		
		//public const bool EmitCallTick = true;
		public const bool EmitCallTick = false;
		
		public const bool EnableTailCalling = true;
		//public const bool EnableTailCall = false;

		public const bool BranchFlagAsLocal = true;
		//public const bool DebugFunctionCreation = true;
		public const bool DebugFunctionCreation = false;
		
		//public const bool EnableGpuSignalsCallback = true;
		//public const bool EnableGpuFinishCallback = true;

		public const bool EnableGpuSignalsCallback = false;
		public const bool EnableGpuFinishCallback = false;

		//public const bool EnableRenderTarget = false;
		public const bool EnableRenderTarget = true;

		//public const bool ImmediateLinking = false;
		public const bool ImmediateLinking = true;

		public const bool AllowCreatingUsedFunctionsInBackground = false;

		public const bool DisableOptimizations = true;
		//public const bool DisableOptimizations = false;

		//public const bool AllowCreatingUsedFunctionsInBackground = true;
	}
}
