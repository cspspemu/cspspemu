using System;
using CSPspEmu.Hle.Attributes;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules._unknownPrx
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public unsafe class scePspNpDrm_user : HleModuleHost
    {
        const int PSP_NPDRM_KEY_LENGHT = 0x10;
        byte[] npDrmKey = new byte[PSP_NPDRM_KEY_LENGHT];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="npDrmKeyAddr"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xA1336091, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNpDrmSetLicenseeKey(byte* npDrmKeyAddr)
        {
            PointerUtils.Memcpy(npDrmKey, npDrmKeyAddr, PSP_NPDRM_KEY_LENGHT);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x9B745542, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNpDrmClearLicenseeKey()
        {
            Array.Clear(npDrmKey, 0, npDrmKey.Length);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x275987D1, FirmwareVersion = 150)]
        public int sceNpDrmRenameCheck(string Name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edataFd"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x08D98894, FirmwareVersion = 150)]
        public int sceNpDrmEdataSetupKey(int edataFd)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edataFd"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x219EF5CC, FirmwareVersion = 150)]
        public int sceNpDrmEdataGetDataSize(int edataFd)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="flags"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x2BAA4294, FirmwareVersion = 150)]
        public int sceNpDrmOpen(string Name, int flags, int permissions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="flags"></param>
        /// <param name="option_addr"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xC618D0B1, FirmwareVersion = 150)]
        public int sceKernelLoadModuleNpDrm(string path, int flags, void* option_addr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xAA5FC85B, FirmwareVersion = 150)]
        public int sceKernelLoadExecNpDrm(string filename, void* option)
        {
            throw new NotImplementedException();
        }
    }
}