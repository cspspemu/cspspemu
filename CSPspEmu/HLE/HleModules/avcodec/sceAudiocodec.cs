using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.avcodec
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public class sceAudiocodec : HleModuleHost
    {
        /// <summary>
        /// 
        /// </summary>
        [HlePspFunction(NID = 0x9D3F790C, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceAudiocodeCheckNeedMem()
        {
            //throw(new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        [HlePspFunction(NID = 0x5B37EB1D, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceAudiocodecInit()
        {
            //throw (new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        [HlePspFunction(NID = 0x70A703F8, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceAudiocodecDecode()
        {
            //throw (new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        [HlePspFunction(NID = 0x3A20A200, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceAudiocodecGetEDRAM()
        {
            //throw (new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        [HlePspFunction(NID = 0x29681260, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceAudiocodecReleaseEDRAM()
        {
            //throw (new NotImplementedException());
        }
    }
}