using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.http
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public class sceSsl : HleModuleHost
    {
        /// <summary>
        /// Init the SSL library.
        /// </summary>
        /// <param name="unknown1">Memory size? Pass 0x28000</param>
        /// <returns>Return 0 on success</returns>
        [HlePspFunction(NID = 0x957ECBE2, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslInit(int unknown1)
        {
            return 0;
        }

        /// <summary>
        /// Terminate the SSL library.
        /// </summary>
        /// <returns>0 on success</returns>
        [HlePspFunction(NID = 0x191CDEFF, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslEnd()
        {
            return 0;
        }

        [HlePspFunction(NID = 0x5BFB6B61, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetNotAfter()
        {
            return 0;
        }

        [HlePspFunction(NID = 0x17A10DCC, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetNotBefore()
        {
            return 0;
        }

        [HlePspFunction(NID = 0x3DD5E023, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetSubjectName()
        {
            return 0;
        }

        [HlePspFunction(NID = 0x1B7C8191, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetIssuerName()
        {
            return 0;
        }

        [HlePspFunction(NID = 0x058D21C0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetNameEntryCount()
        {
            return 0;
        }

        [HlePspFunction(NID = 0xD6D097B4, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetNameEntryInfo()
        {
            return 0;
        }

        [HlePspFunction(NID = 0xB99EDE6A, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetUsedMemoryMax()
        {
            return 0;
        }

        [HlePspFunction(NID = 0x0EB43B06, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetUsedMemoryCurrent()
        {
            return 0;
        }

        [HlePspFunction(NID = 0xF57765D3, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceSslGetKeyUsage()
        {
            return 0;
        }
    }
}