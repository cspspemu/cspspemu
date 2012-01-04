using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.avcodec
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public class sceAudiocodec : HleModuleHost
	{
	}
}
