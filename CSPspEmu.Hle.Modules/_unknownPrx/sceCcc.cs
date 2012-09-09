using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules._unknownPrx //vsh/module/vshmain.prx vsh_module
{
	unsafe public partial class sceCcc : HleModuleHost
	{
        [HlePspFunction(NID = 0x92C05851, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCccEncodeUTF8()
        {
            return 0;
        }

        [HlePspFunction(NID = 0xB7D3C112, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCccStrlenUTF8()
        {
            return 0;
        }


        [HlePspFunction(NID = 0xC6A8BEE2, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCccDecodeUTF8()
        {
            return 0;
        }
	}
}
