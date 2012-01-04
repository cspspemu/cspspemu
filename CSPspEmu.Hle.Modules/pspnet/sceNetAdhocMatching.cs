using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.pspnet
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | (ModuleFlags)0x00010011)]
	public class sceNetAdhocMatching : HleModuleHost
	{
	}
}
