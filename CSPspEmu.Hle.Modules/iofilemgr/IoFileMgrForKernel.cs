using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public class IoFileMgrForKernel : IoFileMgrForUser
	{
	}
}
