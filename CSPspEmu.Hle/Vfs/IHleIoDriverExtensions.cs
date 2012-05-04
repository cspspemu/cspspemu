using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Vfs;

namespace System
{
	unsafe static public class IHleIoDriverExtensions
	{
		static public Stream OpenRead(this IHleIoDriver HleIoDriver, string FileName)
		{
			var HleIoDrvFileArg = new HleIoDrvFileArg("none", HleIoDriver, 0, null);
			HleIoDriver.IoOpen(HleIoDrvFileArg, FileName, HleIoFlags.Read, SceMode.All);
			return HleIoDrvFileArg.GetStream();
		}

		static public bool FileExists(this IHleIoDriver HleIoDriver, string FileName)
		{
			try
			{
				var HleIoDrvFileArg = new HleIoDrvFileArg("none", HleIoDriver, 0, null);
				var SceIoStat = default(SceIoStat);
				HleIoDriver.IoGetstat(HleIoDrvFileArg, FileName, &SceIoStat);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
