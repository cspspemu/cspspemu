using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Modules.modulemgr;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Modules._unknownPrx
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public unsafe class ModuleMgrForKernel : ModuleMgrForUser
    {
        [Inject] HleModuleManager ModuleManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Flags"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xA1A78C58, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceKernelLoadModuleForLoadExecVSHDisc(string FileName, uint Flags,
            ModuleMgrForUser.SceKernelLMOption* option)
        {
            return sceKernelLoadModule(FileName, Flags, option);
        }
    }
}