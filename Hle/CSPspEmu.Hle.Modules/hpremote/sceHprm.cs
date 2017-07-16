using System;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.hpremote
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public unsafe class sceHprm : HleModuleHost
    {
        /// <summary>
        /// Enumeration of the remote keys
        /// </summary>
        [Flags]
        public enum PspHprmKeysEnum : uint
        {
            None = 0,
            PlayPause = 0x1,
            Forward = 0x4,
            Back = 0x8,
            VolumeUp = 0x10,
            VolumeDown = 0x20,
            Hold = 0x80
        };

        /// <summary>
        /// Determines whether the headphones are plugged in.
        /// </summary>
        /// <returns>1 if the headphones are plugged in, else 0.</returns>
        [HlePspFunction(NID = 0x7E69EDA4, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceHprmIsHeadphoneExist()
        {
            //throw(new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Determines whether the microphone is plugged in.
        /// </summary>
        /// <returns>1 if the microphone is plugged in, else 0.</returns>
        [HlePspFunction(NID = 0x219C58F1, FirmwareVersion = 150)]
        public int sceHprmIsMicrophoneExist()
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// Peek at the current being pressed on the remote.
        /// </summary>
        /// <param name="Key">Pointer to the u32 to receive the key bitmap, should be one or more of PspHprmKeys</param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0x1910B327, FirmwareVersion = 150)]
        public int sceHprmPeekCurrentKey(out PspHprmKeysEnum Key)
        {
            Key = PspHprmKeysEnum.None;
            return 0;
        }

        /// <summary>
        /// Determines whether the remote is plugged in.
        /// </summary>
        /// <returns>1 if the remote is plugged in, else 0.</returns>
        [HlePspFunction(NID = 0x208DB1BD, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceHprmIsRemoteExist()
        {
            return 0;
        }

        /// <summary>
        /// Read the current latch data.
        /// </summary>
        /// <param name="Latch">Pointer a to a 4 dword array to contain the latch data.</param>
        /// <returns>Less than 0 on error.</returns>
        [HlePspFunction(NID = 0x40D2F9F0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceHprmReadLatch(uint* Latch)
        {
            throw(new NotImplementedException());
        }
    }
}