using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.audio
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public class sceAudio_Driver : sceAudio
    {
    }
}