using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Dynarec;

namespace CSPspEmu.Core
{
	public class CachedGetMethodCache : PspEmulatorComponent
	{
		[Inject]
		public MethodCacheFast MethodCache;

		[Inject]
		public DynarecFunctionCompilerTask DynarecFunctionCompilerTask;

		public DynarecFunction GetDelegateAt(uint PC)
		{
			var Delegate = MethodCache.TryGetMethodAt(PC);

			if (Delegate == null)
			{
				MethodCache.SetMethodAt(
					PC,
					Delegate = DynarecFunctionCompilerTask.GetFunctionForAddress(PC)
				);
			}

			return Delegate;
		}

	}
}
