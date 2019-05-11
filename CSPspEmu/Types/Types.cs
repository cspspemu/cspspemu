namespace CSPspEmu.Hle
{
    public enum SceUID : int
    {
    }

    public enum SceSize : int
    {
    }

    //public enum SceIoFlags : int { }
    public struct PspIoDrv
    {
    }

    public enum PspModule : uint
    {
        PSP_MODULE_NET_COMMON = 0x0100,
        PSP_MODULE_NET_ADHOC = 0x0101,
        PSP_MODULE_NET_INET = 0x0102,
        PSP_MODULE_NET_PARSEURI = 0x0103,
        PSP_MODULE_NET_PARSEHTTP = 0x0104,
        PSP_MODULE_NET_HTTP = 0x0105,
        PSP_MODULE_NET_SSL = 0x0106,

        // USB Modules
        PSP_MODULE_USB_PSPCM = 0x0200,
        PSP_MODULE_USB_MIC = 0x0201,
        PSP_MODULE_USB_CAM = 0x0202,
        PSP_MODULE_USB_GPS = 0x0203,

        // Audio/video Modules
        PSP_MODULE_AV_AVCODEC = 0x0300,
        PSP_MODULE_AV_SASCORE = 0x0301,
        PSP_MODULE_AV_ATRAC3PLUS = 0x0302,
        PSP_MODULE_AV_MPEGBASE = 0x0303,
        PSP_MODULE_AV_MP3 = 0x0304,
        PSP_MODULE_AV_VAUDIO = 0x0305,
        PSP_MODULE_AV_AAC = 0x0306,
        PSP_MODULE_AV_G729 = 0x0307,

        // NP
        PSP_MODULE_NP_COMMON = 0x0400,
        PSP_MODULE_NP_SERVICE = 0x0401,
        PSP_MODULE_NP_MATCHING2 = 0x0402,

        PSP_MODULE_NP_DRM = 0x0500,

        // IrDA
        PSP_MODULE_IRDA = 0x0600,
    }
}