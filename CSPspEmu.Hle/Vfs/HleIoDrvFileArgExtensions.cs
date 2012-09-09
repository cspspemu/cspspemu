using System.IO;
using CSPspEmu.Hle.Vfs;

namespace System
{
	public static class HleIoDrvFileArgExtensions
	{
		public static Stream GetStream(this HleIoDrvFileArg HleIoDrvFileArg)
		{
			return new FileHandle(HleIoDrvFileArg);
		}
	}
}
