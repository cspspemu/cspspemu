using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Modules.modulemgr;

namespace CSPspEmu.Hle.Modules._unknownPrx
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public class ModuleMgrForKernel : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="Flags"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA1A78C58, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelLoadModuleForLoadExecVSHDisc(string FileName, uint Flags, ModuleMgrForUser.SceKernelLMOption* option)
		{
			return HleState.ModuleManager.GetModule<ModuleMgrForUser>().sceKernelLoadModule(FileName, Flags, option);
		}
	}
}
