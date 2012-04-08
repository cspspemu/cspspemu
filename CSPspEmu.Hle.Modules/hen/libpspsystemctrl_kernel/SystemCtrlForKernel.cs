using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.hen
{
	/// <summary>
	/// 
	/// </summary>
	/// <see cref="http://code.google.com/p/procfw/source/browse/SystemControl/libs/libpspsystemctrl_kernel/SystemCtrlForKernel.S"/>
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public class SystemCtrlForKernel : HleModuleHost
	{
		[HlePspFunction(NID = 0x1C90BECB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sctrlHENSetStartModuleHandler()
		{
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
