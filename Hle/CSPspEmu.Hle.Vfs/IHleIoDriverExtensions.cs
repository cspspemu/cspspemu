using System.IO;
using CSPspEmu.Hle.Vfs;
using System.Collections.Generic;

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

		public static SceIoStat GetStat(this IHleIoDriver HleIoDriver, string FileName)
		{
			var HleIoDrvFileArg = new HleIoDrvFileArg("none", HleIoDriver, 0, null);
			var SceIoStat = default(SceIoStat);
			HleIoDriver.IoGetstat(HleIoDrvFileArg, FileName, &SceIoStat);
			return SceIoStat;
		}

		public static IEnumerable<string> ListDirRecursive(this IHleIoDriver HleIoDriver, string Path)
		{
			foreach (var Item in HleIoDriver.ListDir(Path))
			{
				yield return "/" + (Path + "/" + Item.Name).TrimStart('/');
				if (Item.Stat.Attributes.HasFlag(IOFileModes.Directory))
				{
					foreach (var InnerItem in HleIoDriver.ListDirRecursive(Path + "/" + Item.Name))
					{
						yield return InnerItem;
					}
				}
			}
		}

		public static IEnumerable<HleIoDirent> ListDir(this IHleIoDriver HleIoDriver, string Path)
		{
			var HleIoDrvFileArg = new HleIoDrvFileArg("none", HleIoDriver, 0, null);
			var HleIoDirent = default(HleIoDirent);
			var List = new List<HleIoDirent>();
			HleIoDriver.IoDopen(HleIoDrvFileArg, Path);
			while (HleIoDriver.IoDread(HleIoDrvFileArg, &HleIoDirent) > 0)
			{
				List.Add(HleIoDirent);
			}
			HleIoDriver.IoDclose(HleIoDrvFileArg);
			return List;
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
