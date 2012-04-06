using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Loader;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Modules.iofilemgr;
using CSPspEmu.Hle.Modules.threadman;

namespace CSPspEmu.Hle.Modules.modulemgr
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public class ModuleMgrForUser : HleModuleHost
	{
		public struct SceKernelLMOption
		{
			/// <summary>
			/// 0000 - Size
			/// </summary>
			public uint StructureSize;

			/// <summary>
			/// 0004 -
			/// </summary>
			public int MpidText;

			/// <summary>
			/// 0008 -
			/// </summary>
			public int MpidData;

			/// <summary>
			/// 000C -
			/// </summary>
			public uint Flags;

			/// <summary>
			/// 0010 -
			/// </summary>
			public byte Position;
		
			/// <summary>
			/// 0011 -
			/// </summary>
			public byte Access;

			/// <summary>
			/// 0012 -
			/// </summary>
			public byte _Reserved0;

			/// <summary>
			/// 0013 -
			/// </summary>
			public byte _Reserved1;
		}

		public struct SceKernelModuleInfo
		{
			public int size;
			public byte nsegment;
			public fixed byte reserved[3];
			public fixed int segmentaddr[4];
			public fixed int segmentsize[4];
			public uint entry_addr;
			public uint gp_value;
			public uint text_addr;
			public uint text_size;
			public uint data_size;
			public uint bss_size;
			// The following is only available in the v1.5 firmware and above,
			// but as sceKernelQueryModuleInfo is broken in v1.0 is doesn't matter ;)
			public ElfPsp.ModuleInfo.AtributesEnum attribute;
			public fixed byte version[2];
			public fixed byte name[28];
		}

		public struct SceKernelSMOption
		{
			/// <summary>
			/// 
			/// </summary>
			public uint Size;

			/// <summary>
			/// 
			/// </summary>
			public uint MpidStack;

			/// <summary>
			/// 
			/// </summary>
			public int StackSize;

			/// <summary>
			/// 
			/// </summary>
			public int Priority;

			/// <summary>
			/// 
			/// </summary>
			public uint Attribute;
		}

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

		//public int lastModuleId = 1;

		public HleUidPool<HleModuleGuest> Modules = new HleUidPool<HleModuleGuest>();

		public int sceKernelLoadModuleWithStream(Func<Stream> GetStreamAction, string Path, uint Flags, SceKernelLMOption* SceKernelLMOption)
		{
			var Module = new HleModuleGuest(HleState);

			try
			{
				Path = Path.ToLowerInvariant();

				if (Path.StartsWith(@"disc0:/PSP_GAME/USRDIR/kmodule"))
				{
					throw (new Exception("Ignore kmodule!"));
				}

				if (
					Path.EndsWith(@"/libatrac3plus.prx") ||
					Path.EndsWith(@"/audiocodec.prx") ||
					Path.EndsWith(@"/mpeg.prx") ||
					Path.EndsWith(@"/libfont.prx") ||
				false)
				{
					throw (new Exception("Ignore libatrac3plus.prx!"));
				}

				var ModuleStream = GetStreamAction();

				var Loader = HleState.PspConfig.PspEmulatorContext.GetInstance<ElfPspLoader>();
				var HleModuleGuest = Loader.LoadModule(
					ModuleStream,
					new PspMemoryStream(HleState.CpuProcessor.Memory),
					HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User),
					HleState.ModuleManager,
					"",
					ModuleName: Path,
					IsMainModule: false
				);

				var SceModulePartition = HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.Kernel0).Allocate(sizeof(SceModule));

				var SceModulePtr = (SceModule*)HleState.CpuProcessor.Memory.PspAddressToPointer(SceModulePartition.Low);

				SceModulePtr->Attributes = HleModuleGuest.ModuleInfo.ModuleAtributes;
				SceModulePtr->Version = HleModuleGuest.ModuleInfo.ModuleVersion;
				SceModulePtr->ModuleName = HleModuleGuest.ModuleInfo.Name;
				SceModulePtr->GP = HleModuleGuest.ModuleInfo.GP;
				SceModulePtr->ent_top = HleModuleGuest.ModuleInfo.ExportsStart;
				SceModulePtr->ent_size = HleModuleGuest.ModuleInfo.ExportsEnd - HleModuleGuest.ModuleInfo.ExportsStart;
				SceModulePtr->stub_top = HleModuleGuest.ModuleInfo.ImportsStart;
				SceModulePtr->stub_size = HleModuleGuest.ModuleInfo.ImportsEnd - HleModuleGuest.ModuleInfo.ImportsStart;

				Module.ModuleInfo = HleModuleGuest.ModuleInfo;
				Module.InitInfo = HleModuleGuest.InitInfo;
				Module.Loaded = true;
				Module.SceModuleStructPartition = SceModulePartition;

				//Loader.InitInfo.GP
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
				Module.Loaded = false;
			}

			var ModuleId = Modules.Create(Module);
			Module.ID = ModuleId;
			return ModuleId;
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
		public int sceKernelLoadModule(string Path, uint Flags, SceKernelLMOption* SceKernelLMOption)
		{
			return sceKernelLoadModuleWithStream(() =>
			{
				return HleState.HleIoManager.HleIoWrapper.Open(Path, Vfs.HleIoFlags.Read, Vfs.SceMode.All);
			}, Path, Flags, SceKernelLMOption);
		}

		/// <summary>
		/// Start a loaded module.
		/// </summary>
		/// <param name="ModuleId">The ID of the module returned from LoadModule.</param>
		/// <param name="ArgumentsSize">Length of the args.</param>
		/// <param name="ArgumentsPointer">A pointer to the arguments to the module.</param>
		/// <param name="Status">Returns the status of the start.</param>
		/// <param name="SceKernelSMOption">Pointer to an optional ::SceKernelSMOption structure.</param>
		/// <returns>??? on success, otherwise one of ::PspKernelErrorCodes.</returns>
		[HlePspFunction(NID = 0x50F0C1EC, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelStartModule(CpuThreadState CpuThreadState, int ModuleId, int ArgumentsSize, uint ArgumentsPointer, int* Status, SceKernelSMOption* SceKernelSMOption)
		{
			var Module = Modules.Get(ModuleId);

			if (Module.Loaded)
			{
				var ThreadManForUser = HleState.ModuleManager.GetModule<ThreadManForUser>();

				var NewCpuThreadState = new CpuThreadState(CpuThreadState.CpuProcessor);
				NewCpuThreadState.CopyRegistersFrom(CpuThreadState);
				NewCpuThreadState.GP = Module.InitInfo.GP;
				NewCpuThreadState.CallerModule = Module;

				var ThreadId = (int)ThreadManForUser.sceKernelCreateThread(NewCpuThreadState, "ModuleThread", Module.InitInfo.PC, 10, 1024, PspThreadAttributes.ClearStack, null);
				ThreadManForUser.sceKernelStartThread(NewCpuThreadState, ThreadId, ArgumentsSize, ArgumentsPointer);
			}

			//throw(new NotImplementedException());
			if (Status != null)
			{
				*Status = 0;
			}
			return 1234;
		}

		/// <summary>
		/// Stop a running module.
		/// </summary>
		/// <param name="ModuleId">The UID of the module to stop.</param>
		/// <param name="ArgumentsSize">The length of the arguments pointed to by argp.</param>
		/// <param name="ArgumentsPointer">Pointer to arguments to pass to the module's module_stop() routine.</param>
		/// <param name="Status">Return value of the module's module_stop() routine.</param>
		/// <param name="SceKernelSMOption">Pointer to an optional ::SceKernelSMOption structure.</param>
		/// <returns>
		///		??? on success, otherwise one of ::PspKernelErrorCodes.
		/// </returns>
		[HlePspFunction(NID = 0xD1FF982A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelStopModule(int ModuleId, int ArgumentsSize, void* ArgumentsPointer, int* Status, void* SceKernelSMOption)
		{
			return 0;
			//throw(new NotImplementedException());
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
		/// <param name="ModuleId">The UID of the module to unload.</param>
		/// <returns>
		///		??? on success, otherwise one of ::PspKernelErrorCodes.
		/// </returns>
		[HlePspFunction(NID = 0x2E0911AA, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelUnloadModule(int ModuleId)
		{
			//throw(new NotImplementedException());
			return 0;
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
			return 0x1234;
		}

		/// <summary>
		/// Get module ID from the module that called the API. 
		/// </summary>
		/// <returns>
		/// Greater or equal to zero on success.
		/// </returns>
		[HlePspFunction(NID = 0xF0A26395, FirmwareVersion = 150)]
		public int sceKernelGetModuleId(CpuThreadState CpuThreadState)
		{
			var Module = (HleModuleGuest)CpuThreadState.CallerModule;
			return Module.ID;
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

		/// <summary>
		/// Load a module from the given file UID.
		/// </summary>
		/// <param name="fid">The module's file UID.</param>
		/// <param name="flags">Unused, always 0.</param>
		/// <param name="option">Pointer to an optional ::SceKernelLMOption structure.</param>
		/// <returns>The UID of the loaded module on success, otherwise one of ::PspKernelErrorCodes.</returns>
		[HlePspFunction(NID = 0xB7F46618, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelLoadModuleByID(SceUID FileId, uint Flags, SceKernelLMOption* SceKernelLMOption)
		{
			var IoFileMgrForUser = HleState.ModuleManager.GetModule<IoFileMgrForUser>();
			var Args = IoFileMgrForUser.GetFileArgFromHandle(FileId);
			return sceKernelLoadModuleWithStream(() =>
			{
				return new FileHandle(Args);
			}, Args.FullFileName, Flags, SceKernelLMOption);
		}

		/// <summary>
		/// Query the information about a loaded module from its UID.
		/// </summary>
		/// <remarks>
		/// This fails on v1.0 firmware (and even it worked has a limited structure)
		/// so if you want to be compatible with both 1.5 and 1.0 (and you are running in 
		/// kernel mode) then call this function first then ::pspSdkQueryModuleInfoV1 
		/// if it fails, or make separate v1 and v1.5+ builds.
		/// </remarks>
		/// <param name="ModuleId">The UID of the loaded module.</param>
		/// <param name="ModuleInfo">Pointer to a ::SceKernelModuleInfo structure.</param>
		/// <returns>0 on success, otherwise one of ::PspKernelErrorCodes.</returns>
		[HlePspFunction(NID = 0x748CBED9, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelQueryModuleInfo(int ModuleId, ref SceKernelModuleInfo ModuleInfo)
		{
			var Module = Modules.Get(ModuleId);

			/*
		public struct SceKernelModuleInfo
		{
			public int size;
			public byte nsegment;
			public fixed byte reserved[3];
			public fixed int segmentaddr[4];
			public fixed int segmentsize[4];
			public uint entry_addr;
			public uint gp_value;
			public uint text_addr;
			public uint text_size;
			public uint data_size;
			public uint bss_size;
			// The following is only available in the v1.5 firmware and above,
			// but as sceKernelQueryModuleInfo is broken in v1.0 is doesn't matter ;)
			public ElfPsp.ModuleInfo.AtributesEnum attribute;
			public fixed byte version[2];
			public fixed byte name[28];
		}
		*/

			ModuleInfo.size = sizeof(SceKernelModuleInfo);
			ModuleInfo.nsegment = 0;
			/*
			ModuleInfo.reserved[0] = 0;
			ModuleInfo.reserved[1] = 0;
			ModuleInfo.reserved[2] = 0;
			ModuleInfo.segmentaddr[0] = 0;
			*/
			ModuleInfo.entry_addr = Module.InitInfo.PC;
			ModuleInfo.gp_value = Module.InitInfo.GP;
			ModuleInfo.attribute = Module.ModuleInfo.ModuleAtributes;

			Console.Error.WriteLine("sceKernelQueryModuleInfo : PARTIALLY IMPLEMENTED!");

			//ModuleInfo = Module
			//return 0;
			//throw(new NotImplementedException("sceKernelQueryModuleInfo"));
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ArgumentSize"></param>
		/// <param name="ArgumentPointer"></param>
		/// <param name="StatusPointer"></param>
		/// <param name="OptionsAddress"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xCC1D3699, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelStopUnloadSelfModule(int ArgumentSize, void* ArgumentPointer, int* StatusPointer, void* OptionsAddress)
		{
			throw (new NotImplementedException("sceKernelStopUnloadSelfModule"));
			//return 0;
		}
	}
}
