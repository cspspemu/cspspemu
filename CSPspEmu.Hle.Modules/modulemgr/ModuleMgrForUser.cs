using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.modulemgr
{
	unsafe public class ModuleMgrForUser : HleModuleHost
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


		/// <summary>
		/// Load a module.
		/// </summary>
		/// <remarks>
		/// This function restricts where it can load from (such as from flash0) 
		/// unless you call it in kernel mode. It also must be called from a thread.
		/// </remarks>
		/// <param name="Path">The path to the module to load.</param>
		/// <param name="Flags">Unused, always 0 .</param>
		/// <param name="SceKernelLMOption">Pointer to a mod_param_t structure. Can be NULL.</param>
		/// <returns>The UID of the loaded module on success, otherwise one of ::PspKernelErrorCodes.</returns>
		[HlePspFunction(NID = 0x977DE386, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelLoadModule(string Path, int Flags, void* SceKernelLMOption)
		{
			//throw(new NotImplementedException());
			return 1;
		}

		/// <summary>
		/// Start a loaded module.
		/// </summary>
		/// <param name="ModuleId">The ID of the module returned from LoadModule.</param>
		/// <param name="ArgumentsCount">Length of the args.</param>
		/// <param name="ArgumentsPointer">A pointer to the arguments to the module.</param>
		/// <param name="Status">Returns the status of the start.</param>
		/// <param name="SceKernelSMOption">Pointer to an optional ::SceKernelSMOption structure.</param>
		/// <returns>??? on success, otherwise one of ::PspKernelErrorCodes.</returns>
		[HlePspFunction(NID = 0x50F0C1EC, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelStartModule(int ModuleId, uint ArgumentsCount, uint ArgumentsPointer, int *Status, void *SceKernelSMOption)
		{
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
