using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Vfs;

namespace System
{
	static public class IHleIoDriverExtensions
	{
		static public Stream OpenRead(this IHleIoDriver HleIoDriver, string FileName)
		{
			var HleIoDrvFileArg = new HleIoDrvFileArg("none", HleIoDriver, 0, null);
			HleIoDriver.IoOpen(HleIoDrvFileArg, FileName, HleIoFlags.Read, SceMode.All);
			return HleIoDrvFileArg.GetStream();
		}
	}
}
