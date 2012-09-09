using System.IO;
using CSPspEmu.Hle.Vfs;

namespace System
{
	static public class HleIoDrvFileArgExtensions
	{
		static public Stream GetStream(this HleIoDrvFileArg HleIoDrvFileArg)
		{
			return new FileHandle(HleIoDrvFileArg);
		}
	}
}
