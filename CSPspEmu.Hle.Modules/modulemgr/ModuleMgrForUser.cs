using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.modulemgr
{
	public class ModuleMgrForUser : HleModuleHost
	{
		/// <summary>
		/// Stop and unload the current module.
		/// </summary>
		/// <param name="unknown">Unknown (I've seen 1 passed).</param>
		/// <param name="argsize">Size (in bytes) of the arguments that will be passed to module_stop().</param>
		/// <param name="argp">Pointer to arguments that will be passed to module_stop().</param>
		/// <returns>??? on success, otherwise one of ::PspKernelErrorCodes.</returns>
		[HlePspFunction(NID = 0xD675EBB8, FirmwareVersion = 150)]
		public int sceKernelSelfStopUnloadModule(int unknown, int argsize, uint argp)
		{
			throw (new SceKernelSelfStopUnloadModuleException());
		}
	}
}
