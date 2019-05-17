using CSharpUtils;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.loadexec
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public unsafe class sceDmac : HleModuleHost
    {
        /// <summary>
        /// Copies data using the internal DMAC. Should be faster than a memcpy,
        /// but requires that data to be copied is no more in the cache, so usually
        /// you should issue a oslUncacheData on the source and destination addresses
        /// else very strange bugs may happen.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x617F3FE6, FirmwareVersion = 150)]
        public int sceDmacMemcpy(byte* destination, byte* source, int size)
        {
            PointerUtils.Memcpy(destination, source, size);
            return 0;
        }

        //mixin(registerFunction!(0xD97F94D8, sceDmacTryMemcpy));
    }
}