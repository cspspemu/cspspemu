using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.sysmem
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00000011)]
	public class SysMemForKernel : SysMemUserForUser
	{
	}
}
