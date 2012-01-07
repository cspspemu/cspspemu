using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.openpsid
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceOpenPSID : HleModuleHost
	{
		public struct PspOpenPSID
		{
			public fixed byte Data[16];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="OpenPSID"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xC69BEBCE, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceOpenPSIDGetOpenPSID(PspOpenPSID* OpenPSID)
		{
			var DummyPspOpenPSID = default(PspOpenPSID);
			int n = 0;
			foreach (var Byte in new byte[] { 0x10, 0x02, 0xA3, 0x44, 0x13, 0xF5, 0x93, 0xB0, 0xCC, 0x6E, 0xD1, 0x32, 0x27, 0x85, 0x0F, 0x9D })
			{
				DummyPspOpenPSID.Data[n++] = Byte;
			}
			*OpenPSID = DummyPspOpenPSID;
			return 0;
		}
	}
}
