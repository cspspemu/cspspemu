﻿using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.lowio
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public class sceSysreg_driver : HleModuleHost
    {
    }
}