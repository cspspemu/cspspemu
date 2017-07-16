using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.modulemgr
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public class sceModuleManager : HleModuleHost
    {
    }
}