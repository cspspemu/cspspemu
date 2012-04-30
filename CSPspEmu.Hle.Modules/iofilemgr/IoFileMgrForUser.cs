using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class IoFileMgrForUser : HleModuleHost
	{
		[Inject]
		HleIoManager HleIoManager;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileHandle"></param>
		/// <returns></returns>
		public HleIoDrvFileArg GetFileArgFromHandle(SceUID FileHandle)
		{
			return HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
		}

		public override void Dispose()
		{
			HleIoManager.HleIoDrvFileArgPool.RemoveAll();
			base.Dispose();
		}
	}
}
