using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSPspEmu.Hle.Vfs;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public partial class IoFileMgrForUser : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileHandle"></param>
		/// <returns></returns>
		private HleIoDrvFileArg GetFileArgFromHandle(int FileHandle)
		{
			return HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
		}
	}
}
