using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Core.Components.Rtc;

namespace CSPspEmu.Hle.Modules.display
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public class sceDisplay : HleModuleHost
    {
        [Inject] PspDisplay PspDisplay;

        [Inject] DisplayConfig DisplayConfig;

        [Inject] PspRtc PspRtc;

        [Inject] HleThreadManager ThreadManager;

        /// <summary>
        /// Set display mode
        /// </summary>
        /// <param name="Mode">Display mode, normally 0.</param>
        /// <param name="Width">Width of screen in pixels.</param>
        /// <param name="Height">Height of screen in pixels.</param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x0E20F177, FirmwareVersion = 150)]
        public int sceDisplaySetMode(int Mode, int Width, int Height)
        {
            //Console.WriteLine("sceDisplay.sceDisplaySetMode");

            PspDisplay.CurrentInfo.Mode = Mode;
            PspDisplay.CurrentInfo.Width = Width;
            PspDisplay.CurrentInfo.Height = Height;
            return 0;
        }

        private int _waitVblankCB(CpuThreadState CpuThreadState, bool HandleCallbacks, int CycleCount, bool Start)
        {
            if (CycleCount <= 0)
            {
                throw(new SceKernelException(SceKernelErrors.ERROR_INVALID_VALUE));
            }

            int Wait = 1;

            //if (DisplayConfig.VerticalSynchronization && (Start || !PspDisplay.IsVblank))
            if (DisplayConfig.VerticalSynchronization)
            {
                ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.Display, "sceDisplayWaitVblankStart",
                    null, (WakeUp) =>
                    {
                        Action Next = null;
                        Action WakeUpRemoveCallback = () =>
                        {
                            PspDisplay.VBlankCallback -= Next;
                            WakeUp();
                        };
                        Next = () =>
                        {
                            if (!Start && PspDisplay.IsVblank)
                            {
                                WakeUpRemoveCallback();
                            }
                            else
                            {
                                //CycleCount--;
                                if (CycleCount-- <= 0)
                                {
                                    Wait = 0;
                                    WakeUpRemoveCallback();
                                }
                            }
                        };
                        PspDisplay.VBlankCallback += Next;
                        Next();
                    }, HandleCallbacks: HandleCallbacks);
            }

            return Wait;
        }

        /// <summary>
        /// Test wheter VBLANK is active
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x4D4E10EC, FirmwareVersion = 150)]
        public bool sceDisplayIsVblank()
        {
            return PspDisplay.IsVblank;
        }


        /// <summary>
        /// Display set framebuf
        /// </summary>
        /// <param name="Address">Address of start of framebuffer</param>
        /// <param name="BufferWidth">buffer width (must be power of 2)</param>
        /// <param name="PixelFormat">One of ::PspDisplayPixelFormats.</param>
        /// <param name="Sync">One of ::PspDisplaySetBufSync</param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x289D82FE, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceDisplaySetFrameBuf(uint Address, int BufferWidth, GuPixelFormats PixelFormat,
            PspDisplay.SyncMode Sync)
        {
            Action UpdateInfo = () =>
            {
                PspDisplay.CurrentInfo.Enabled = (Address != 0);
                if (PspDisplay.CurrentInfo.Enabled)
                {
                    PspDisplay.CurrentInfo.FrameAddress = Address;
                    PspDisplay.CurrentInfo.BufferWidth = BufferWidth;
                    PspDisplay.CurrentInfo.PixelFormat = PixelFormat;
                }
            };
            switch (Sync)
            {
                case PspDisplay.SyncMode.Immediate:
                    UpdateInfo();
                    break;
                case PspDisplay.SyncMode.NextFrame:
                    ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.Display, "sceDisplaySetFrameBuf",
                        null, (WakeUp) =>
                        {
                            PspDisplay.VBlankCallbackOnce(() =>
                            {
                                UpdateInfo();
                                WakeUp();
                            });
                        }, HandleCallbacks: false);
                    break;
                default:
                    throw(new NotImplementedException("Not implemented " + Sync));
            }
            return 0;
        }

        /// <summary>
        /// Get current HSYNC count
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x773DD3A3, FirmwareVersion = 150)]
        [HlePspNotImplemented(Notice = false)]
        public int sceDisplayGetCurrentHcount()
        {
            return PspDisplay.GetHCount();
        }

        /// <summary>
        /// Get number of frames per second
        /// </summary>
        /// <see cref="http://forums.ps2dev.org/viewtopic.php?t=9168"/>
        /// <remarks>(pixel_clk_freq * cycles_per_pixel)/(row_pixels * column_pixel)</remarks>
        /// <returns></returns>
        [HlePspFunction(NID = 0xDBA6C4C4, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public float sceDisplayGetFramePerSec()
        {
            return (float) (PspDisplay.ProcessedPixelsPerSecond * PspDisplay.CyclesPerPixel /
                            (PspDisplay.PixelsInARow * PspDisplay.NumberOfRows));
        }

        /// <summary>
        /// Get accumlated HSYNC count
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x210EAB3A, FirmwareVersion = 150)]
        [HlePspNotImplemented(Notice = false)]
        public int sceDisplayGetAccumulatedHcount()
        {
            return (int) (sceDisplayGetCurrentHcount() + sceDisplayGetVcount() * PspDisplay.HCountPerVblank);
        }

        /// <summary>
        /// Number of vertical blank pulses up to now
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x9C6EAAD7, FirmwareVersion = 150)]
        public uint sceDisplayGetVcount()
        {
            return (uint) PspDisplay.VblankCount;
        }

        /// <summary>
        /// Get Display Framebuffer information
        /// </summary>
        /// <param name="topaddr">pointer to void* to receive address of start of framebuffer</param>
        /// <param name="bufferwidth">pointer to int to receive buffer width (must be power of 2)</param>
        /// <param name="pixelformat">pointer to int to receive one of ::PspDisplayPixelFormats.</param>
        /// <param name="sync">One of ::PspDisplaySetBufSync</param>
        /// <returns>0 on success</returns>
        [HlePspFunction(NID = 0xEEDA2E54, FirmwareVersion = 150)]
        //public int sceDisplayGetFrameBuf(uint* topaddr, int* bufferwidth, PspDisplayPixelFormats* pixelformat, PspDisplaySetBufSync sync)
        public int sceDisplayGetFrameBuf(ref uint topaddr, ref int bufferwidth, ref GuPixelFormats pixelformat,
            uint sync)
        {
            topaddr = PspDisplay.CurrentInfo.FrameAddress;
            bufferwidth = PspDisplay.CurrentInfo.BufferWidth;
            pixelformat = PspDisplay.CurrentInfo.PixelFormat;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CycleNum">Number of VSYNCs to wait before blocking the thread on VBLANK.</param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x40F1469C, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceDisplayWaitVblankStartMulti(int CycleNum)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holdMode"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x7ED59BC4, FirmwareVersion = 150)]
        [HlePspNotImplemented(Notice = false)]
        public int sceDisplaySetHoldMode(int holdMode)
        {
            return 0;
        }

        /// <summary>
        /// Get display mode
        /// </summary>
        /// <param name="ModeOut">Integer to receive the current mode.</param>
        /// <param name="WidthOut">Integer to receive the current width.</param>
        /// <param name="HeightOut">Integer to receive the current height.</param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xDEA197D4, FirmwareVersion = 150)]
        public int sceDisplayGetMode(out int ModeOut, out int WidthOut, out int HeightOut)
        {
            ModeOut = PspDisplay.CurrentInfo.Mode;
            WidthOut = PspDisplay.CurrentInfo.Width;
            HeightOut = PspDisplay.CurrentInfo.Height;

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CpuThreadState"></param>
        /// <param name="CycleCount">Number of VSYNCs to wait before blocking the thread on VBLANK.</param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x40F1469C, FirmwareVersion = 500, CheckInsideInterrupt = true)]
        public int sceDisplayWaitVblankStartMulti(CpuThreadState CpuThreadState, int CycleCount)
        {
            return _waitVblankCB(CpuThreadState, HandleCallbacks: false, CycleCount: CycleCount, Start: true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CpuThreadState"></param>
        /// <param name="CycleCount">Number of VSYNCs to wait before blocking the thread on VBLANK.</param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x77ED8B3A, FirmwareVersion = 500, CheckInsideInterrupt = true)]
        public int sceDisplayWaitVblankStartMultiCB(CpuThreadState CpuThreadState, int CycleCount)
        {
            return _waitVblankCB(CpuThreadState, HandleCallbacks: true, CycleCount: CycleCount, Start: true);
        }

        /// <summary>
        /// Wait for vertical blank start
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x984C27E7, FirmwareVersion = 150)]
        public int sceDisplayWaitVblankStart(CpuThreadState CpuThreadState)
        {
            return _waitVblankCB(CpuThreadState, HandleCallbacks: false, CycleCount: 1, Start: true);
        }

        /// <summary>
        /// Wait for vertical blank start with callback
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x46F186C3, FirmwareVersion = 150)]
        public int sceDisplayWaitVblankStartCB(CpuThreadState CpuThreadState)
        {
            return _waitVblankCB(CpuThreadState, HandleCallbacks: true, CycleCount: 1, Start: true);
        }

        /// <summary>
        /// Wait for vertical blank
        /// </summary>
        [HlePspFunction(NID = 0x36CDFADE, FirmwareVersion = 150)]
        public int sceDisplayWaitVblank(CpuThreadState CpuThreadState)
        {
            return _waitVblankCB(CpuThreadState, HandleCallbacks: false, CycleCount: 1, Start: false);
        }

        /// <summary>
        /// Wait for vertical blank with callback
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x8EB9EC49, FirmwareVersion = 150)]
        public int sceDisplayWaitVblankCB(CpuThreadState CpuThreadState)
        {
            return _waitVblankCB(CpuThreadState, HandleCallbacks: true, CycleCount: 1, Start: false);
        }

        /// <summary>
        /// Get whether or not frame buffer is being displayed
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0xB4F378FA, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceDisplayIsForeground()
        {
            if (PspDisplay.CurrentInfo.FrameAddress == 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}