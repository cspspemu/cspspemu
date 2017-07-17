using CSPspEmu.Core;
using CSPspEmu.Core.Components.Controller;
using CSPspEmu.Core.Types;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.ctrl
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public class sceCtrl_driver : sceCtrl
    {
    }

    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public unsafe class sceCtrl : HleModuleHost
    {
        [Inject] PspController PspController;

        protected void _ReadCount(SceCtrlData* SceCtrlData, int Count, bool Peek, bool Positive)
        {
            for (int n = 0; n < Count; n++) SceCtrlData[n] = PspController.GetSceCtrlDataAt(n);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SceCtrlData"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x3A622550, FirmwareVersion = 150)]
        public int sceCtrlPeekBufferPositive(SceCtrlData* SceCtrlData, int Count)
        {
            _ReadCount(SceCtrlData, Count, Peek: true, Positive: true);
            return Count;
        }

        // sceCtrlReadBufferPositive () is blocking and waits for vblank (slower).
        /// <summary>
        /// Read buffer positive
        /// </summary>
        /// <example>
        ///		SceCtrlData pad;
        ///		
        ///		sceCtrlSetSamplingCycle(0);
        ///		sceCtrlSetSamplingMode(1);
        ///		sceCtrlReadBufferPositive(&amp;pad, 1);
        ///		// Do something with the read controller data
        /// </example>
        /// <param name="SceCtrlData">Pointer to a <see cref="SceCtrlData"/> structure used hold the returned pad data.</param>
        /// <param name="Count">Number of <see cref="SceCtrlData"/> buffers to read.</param>
        /// <returns>Count?</returns>
        [HlePspFunction(NID = 0x1F803938, FirmwareVersion = 150, SkipLog = true)]
        public int sceCtrlReadBufferPositive(SceCtrlData* SceCtrlData, int Count)
        {
            _ReadCount(SceCtrlData, Count, Peek: false, Positive: true);
            return Count;
        }

        /// <summary>
        /// Set the controller mode.
        /// </summary>
        /// <param name="SamplingMode">
        /// One of ::PspCtrlMode.
        /// PSP_CTRL_MODE_DIGITAL = 0
        /// PSP_CTRL_MODE_ANALOG  = 1
        /// 
        /// PSP_CTRL_MODE_DIGITAL is the same as PSP_CTRL_MODE_ANALOG
        /// except that doesn't update Lx and Ly values. Setting them to 0x80.
        /// </param>
        /// <returns>The previous mode.</returns>
        [HlePspFunction(NID = 0x1F4011E6, FirmwareVersion = 150)]
        public PspController.SamplingModeEnum sceCtrlSetSamplingMode(PspController.SamplingModeEnum SamplingMode)
        {
            try
            {
                return PspController.SamplingMode;
            }
            finally
            {
                PspController.SamplingMode = SamplingMode;
            }
        }

        /// <summary>
        /// Gets the current controller mode.
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0xDA6B76A1, FirmwareVersion = 150)]
        public PspController.SamplingModeEnum sceCtrlGetSamplingMode()
        {
            return PspController.SamplingMode;
        }

        /// <summary>
        /// Set the controller cycle setting.
        /// </summary>
        /// <param name="SamplingCycle">
        /// Cycle. Normally set to 0.
        /// 
        /// @TODO Unknown what this means exactly.
        /// </param>
        /// <returns>The previous cycle setting.</returns>
        [HlePspFunction(NID = 0x6A2774F3, FirmwareVersion = 150)]
        public int sceCtrlSetSamplingCycle(int SamplingCycle)
        {
            try
            {
                return PspController.SamplingCycle;
            }
            finally
            {
                PspController.SamplingCycle = SamplingCycle;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentLatch"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xB1D0E5CD, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceCtrlPeekLatch(SceCtrlLatch* CurrentLatch)
        {
            var ButtonsNew = PspController.GetSceCtrlDataAt(0).Buttons;
            var ButtonsOld = LastLatchData.Buttons;
            var ButtonsChanged = ButtonsOld ^ ButtonsNew;

            CurrentLatch->uiBreak = ButtonsOld & ButtonsChanged;
            CurrentLatch->uiMake = ButtonsNew & ButtonsChanged;
            CurrentLatch->uiPress = ButtonsNew;
            CurrentLatch->uiRelease = (ButtonsOld & ~ButtonsNew) & ButtonsChanged;

            return PspController.LatchSamplingCount;
        }

        SceCtrlData LastLatchData;

        /// <summary>
        /// Obtains information about currentLatch.
        /// </summary>
        /// <param name="CurrentLatch">Pointer to SceCtrlLatch to store the result.</param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x0B588501, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceCtrlReadLatch(SceCtrlLatch* CurrentLatch)
        {
            try
            {
                return sceCtrlPeekLatch(CurrentLatch);
            }
            finally
            {
                LastLatchData = PspController.GetSceCtrlDataAt(0);
                PspController.LatchSamplingCount = 0;
            }
        }

        /// <summary>
        /// Set analog threshold relating to the idle timer.
        /// </summary>
        /// <param name="idlereset">Movement needed by the analog to reset the idle timer.</param>
        /// <param name="idleback">Movement needed by the analog to bring the PSP back from an idle state.</param>
        /// <remarks>
        /// Set to -1 for analog to not cancel idle timer.
        /// Set to 0 for idle timer to be cancelled even if the analog is not moved.
        /// Set between 1 - 128 to specify the movement on either axis needed by the analog to fire the event.
        /// </remarks>
        /// <returns>
        ///		Less than 0 on error
        /// </returns>
        [HlePspFunction(NID = 0xA7144800, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCtrlSetIdleCancelThreshold(int idlereset, int idleback)
        {
            return 0;
        }

        [HlePspFunction(NID = 0xC152080A, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCtrlPeekBufferNegative()
        {
            return 0;
        }

        [HlePspFunction(NID = 0xA68FD260, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCtrlClearRapidFire()
        {
            return 0;
        }

        /// <summary>
        /// Get the idle threshold values.
        /// </summary>
        /// <param name="idlerest">Movement needed by the analog to reset the idle timer.</param>
        /// <param name="idleback">Movement needed by the analog to bring the PSP back from an idle state.</param>
        /// <returns>&lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x687660FA, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCtrlGetIdleCancelThreshold(int*idlerest, int*idleback)
        {
            return 0;
        }

        [HlePspFunction(NID = 0x6841BE1A, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCtrlSetRapidFire()
        {
            return 0;
        }

        [HlePspFunction(NID = 0x60B81F86, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCtrlReadBufferNegative()
        {
            return 0;
        }

        /// <summary>
        /// Get the controller current cycle setting.
        /// </summary>
        /// <param name="pcycle">Return value.</param>
        /// <returns>Return 0</returns>
        [HlePspFunction(NID = 0x02BAAD91, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceCtrlGetSamplingCycle(int*pcycle)
        {
            return 0;
        }

        /// <summary>
        /// Controller latch.
        /// </summary>
        public struct SceCtrlLatch
        {
            /// <summary>
            /// A bit fields of buttons just pressed (since last call?)
            /// </summary>
            public PspCtrlButtons uiMake;

            /// <summary>
            /// A bit fields of buttons just released (since last call?)
            /// </summary>
            public PspCtrlButtons uiBreak;

            /// <summary>
            /// Same has SceCtrlData.Buttons?
            /// </summary>
            public PspCtrlButtons uiPress;

            /// <summary>
            /// A bit field of buttons released 
            /// </summary>
            public PspCtrlButtons uiRelease;
        }
    }
}