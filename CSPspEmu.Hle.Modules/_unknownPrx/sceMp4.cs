using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Modules.modulemgr;

namespace CSPspEmu.Hle.Modules._unknownPrx
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public class sceMp4 : ModuleMgrForUser
	{
		[HlePspFunction(NID = 0x68651CBC, FirmwareVersion = 150, CheckInsideInterrupt = true)]
		[HlePspNotImplemented]
		public int sceMp4Init(bool unk1, bool unk2)
		{
			return 0;
		}

		[HlePspFunction(NID = 0x9042B257, FirmwareVersion = 150, CheckInsideInterrupt = true)]
		[HlePspNotImplemented]
		public int sceMp4Finish()
		{
			return 0;
		}

		[HlePspFunction(NID = 0xB1221EE7, FirmwareVersion = 150, CheckInsideInterrupt = true)]
		[HlePspNotImplemented]
		public int sceMp4Create()
		{
			return 0;
		}

		[HlePspFunction(NID = 0x538C2057, FirmwareVersion = 150, CheckInsideInterrupt = true)]
		[HlePspNotImplemented]
		public int sceMp4Delete()
		{
			return 0;
		}
	}
}
