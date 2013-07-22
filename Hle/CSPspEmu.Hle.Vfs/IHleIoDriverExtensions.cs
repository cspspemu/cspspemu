using System.IO;
using CSPspEmu.Hle.Vfs;

namespace System
{
	public static unsafe class IHleIoDriverExtensions
	{
		public static Stream OpenRead(this IHleIoDriver HleIoDriver, string FileName)
		{
			var HleIoDrvFileArg = new HleIoDrvFileArg("none", HleIoDriver, 0, null);
			HleIoDriver.IoOpen(HleIoDrvFileArg, FileName, HleIoFlags.Read, SceMode.All);
			return HleIoDrvFileArg.GetStream();
		}

		public static bool FileExists(this IHleIoDriver HleIoDriver, string FileName)
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
