using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
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
