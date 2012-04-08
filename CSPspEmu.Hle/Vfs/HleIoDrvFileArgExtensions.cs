using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
