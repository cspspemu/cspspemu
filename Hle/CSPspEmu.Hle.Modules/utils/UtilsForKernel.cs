using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.utils
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public class UtilsForKernel : UtilsForUser
    {
    }
}