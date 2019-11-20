using System;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.pspnet
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | (ModuleFlags) 0x00010011)]
    public unsafe class sceNetAdhocDiscover : HleModuleHost
    {
        [HlePspFunction(NID = 0x941B3877, FirmwareVersion = 150)]
        public void sceNetAdhocDiscoverInitStart()
        {
            throw new NotImplementedException();
        }

        [HlePspFunction(NID = 0x52DE1B97, FirmwareVersion = 150)]
        public void sceNetAdhocDiscoverUpdate()
        {
            throw new NotImplementedException();
        }

        [HlePspFunction(NID = 0x944DDBC6, FirmwareVersion = 150)]
        public void sceNetAdhocDiscoverGetStatus()
        {
            throw new NotImplementedException();
        }

        [HlePspFunction(NID = 0xA2246614, FirmwareVersion = 150)]
        public void sceNetAdhocDiscoverTerm()
        {
            throw new NotImplementedException();
        }

        [HlePspFunction(NID = 0xF7D13214, FirmwareVersion = 150)]
        public void sceNetAdhocDiscoverStop()
        {
            throw new NotImplementedException();
        }

        [HlePspFunction(NID = 0xA423A21B, FirmwareVersion = 150)]
        public void sceNetAdhocDiscoverRequestSuspend()
        {
            throw new NotImplementedException();
        }
    }
}