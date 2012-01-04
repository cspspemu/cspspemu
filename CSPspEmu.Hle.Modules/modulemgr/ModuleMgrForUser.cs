using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.modulemgr
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
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

		/// <summary>
		/// Stop a running module.
		/// </summary>
		/// <param name="modid">The UID of the module to stop.</param>
		/// <param name="argsize">The length of the arguments pointed to by argp.</param>
		/// <param name="argp">Pointer to arguments to pass to the module's module_stop() routine.</param>
		/// <param name="status">Return value of the module's module_stop() routine.</param>
		/// <param name="SceKernelSMOption">Pointer to an optional ::SceKernelSMOption structure.</param>
		/// <returns>
		///		??? on success, otherwise one of ::PspKernelErrorCodes.
		/// </returns>
		[HlePspFunction(NID = 0xD1FF982A, FirmwareVersion = 150)]
		public int sceKernelStopModule(int modid, int argsize, void* argp, int* status, void* SceKernelSMOption)
		{
			throw(new NotImplementedException());
			/*
			Module pspModule = hleEmulatorState.uniqueIdFactory.get!Module(modid);

			unimplemented_notice();
			logError("Not implemented sceKernelStopModule!!");
			return 0;
			*/
		}

		/// <summary>
		/// Unload a stopped module.
		/// </summary>
		/// <param name="modid">The UID of the module to unload.</param>
		/// <returns>
		///		??? on success, otherwise one of ::PspKernelErrorCodes.
		/// </returns>
		[HlePspFunction(NID = 0x2E0911AA, FirmwareVersion = 150)]
		public int sceKernelUnloadModule(int modid)
		{
			throw(new NotImplementedException());
			/*
			Module pspModule = hleEmulatorState.uniqueIdFactory.get!Module(modid);

			unimplemented_notice();
			logError("Not implemented sceKernelUnloadModule!!");
		
			//pspModule.
		
			//hleEmulatorState.moduleManager.unloadModu
			return 0;
			*/
		}

		/// <summary>
		/// Gets a module by its loaded address.
		/// </summary>
		/// <param name="Address"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xD8B73127, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelGetModuleIdByAddress(uint Address)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Get module ID from the module that called the API. 
		/// </summary>
		/// <returns>
		/// Greater or equal to zero on success.
		/// </returns>
		[HlePspFunction(NID = 0xF0A26395, FirmwareVersion = 150)]
		public uint sceKernelGetModuleId()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x8F2DF740, FirmwareVersion = 150)]
		public uint sceKernelStopUnloadSelfModuleWithStatus()
		{
			throw(new NotImplementedException());
		}
	}
}
