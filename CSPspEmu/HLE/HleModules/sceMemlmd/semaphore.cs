using System;
using CSPspEmu.Hle.Attributes;
using Kirk = CSPspEmu.Core.Components.Crypto.Kirk;

namespace CSPspEmu.Hle.Modules.kirk
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public unsafe partial class semaphore : HleModuleHost
    {
        [Inject] Kirk Kirk;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x00EEC06A, FirmwareVersion = 150)]
        [HlePspNotImplemented()]
        public int sceUtilsBufferCopy()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        [HlePspFunction(NID = 0x8EEB7BF2, FirmwareVersion = 150)]
        [HlePspNotImplemented()]
        public void sceUtilsBufferCopyByPolling()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OutputBuffer"></param>
        /// <param name="OutputSize"></param>
        /// <param name="InputBuffer"></param>
        /// <param name="InputSize"></param>
        /// <param name="Command"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x4C537C72, FirmwareVersion = 150)]
        public int sceUtilsBufferCopyWithRange(byte* OutputBuffer, int OutputSize, byte* InputBuffer, int InputSize,
            int Command)
        {
            return Kirk.SceUtilsBufferCopyWithRange(OutputBuffer, OutputSize, InputBuffer, InputSize, Command);
        }

        /// <summary>
        /// 
        /// </summary>
        [HlePspFunction(NID = 0x77E97079, FirmwareVersion = 150)]
        [HlePspNotImplemented()]
        public void sceUtilsBufferCopyByPollingWithRange()
        {
            throw new NotImplementedException();
        }
    }
}