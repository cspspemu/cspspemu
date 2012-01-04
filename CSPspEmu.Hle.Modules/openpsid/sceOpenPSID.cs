using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.openpsid
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceOpenPSID : HleModuleHost
	{
		public struct PspOpenPSID
		{
			public fixed byte data[16];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="openpsid"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xC69BEBCE, FirmwareVersion = 150)]
		public int sceOpenPSIDGetOpenPSID(PspOpenPSID* openpsid)
		{
			throw(new NotImplementedException());
		}
	}
}
