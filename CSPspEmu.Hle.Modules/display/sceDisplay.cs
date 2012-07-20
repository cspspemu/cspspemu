using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Attributes;
using System.Diagnostics;

namespace CSPspEmu.Hle.Modules.display
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public class sceDisplay : HleModuleHost
	{
		[Inject]
		public PspDisplay PspDisplay;

		[Inject]
		public PspRtc PspRtc;

		[Inject]
		public HleThreadManager ThreadManager;

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

		int LastVblankCount = 0;
		DateTime LastWaitVblankStart = DateTime.MinValue;

		private int _waitVblankCB(CpuThreadState CpuThreadState, bool HandleCallbacks, int CycleCount)
		{
			if (PspConfig.VerticalSynchronization && LastVblankCount != PspDisplay.VblankCount)
			{
				var SleepThread = ThreadManager.Current;

				SleepThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Display, "sceDisplayWaitVblankStart", null, (WakeUpCallbackDelegate) =>
				{
#if true
					PspRtc.RegisterTimerInOnce(TimeSpan.FromMilliseconds(1000 / 60), () =>
					{
						WakeUpCallbackDelegate();
					});
#else
					if (LastWaitVblankStart == DateTime.MinValue)
					{
						LastWaitVblankStart = PspRtc.UpdatedCurrentDateTime;
					}
					PspRtc.RegisterTimerAtOnce(LastWaitVblankStart + TimeSpan.FromMilliseconds(1000 / 60), () =>
					{
						WakeUpCallbackDelegate();
					});
					LastWaitVblankStart = PspRtc.UpdatedCurrentDateTime;
#endif
				}, HandleCallbacks: HandleCallbacks);

				/*
				SleepThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Display, "sceDisplayWaitVblankStart", (WakeUpCallbackDelegate) =>
				{
					//PspDisplay.VBlankEvent
					PspDisplay.VBlankEvent.CallbackOnStateOnce(() =>
					{
						LastVblankCount = PspDisplay.VblankCount;
						WakeUpCallbackDelegate();
					});
				});
				*/
			}

			return 0;
		}

		private int _sceDisplayWaitVblankStartCB(CpuThreadState CpuThreadState, bool HandleCallbacks)
		{
			return _waitVblankCB(CpuThreadState, HandleCallbacks, CycleCount: 1);
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
		/// Wait for vertical blank start
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x984C27E7, FirmwareVersion = 150)]
		public int sceDisplayWaitVblankStart(CpuThreadState CpuThreadState)
		{
			return _sceDisplayWaitVblankStartCB(CpuThreadState, HandleCallbacks: false);
		}

		/// <summary>
		/// Wait for vertical blank start with callback
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x46F186C3, FirmwareVersion = 150)]
		public int sceDisplayWaitVblankStartCB(CpuThreadState CpuThreadState)
		{
			return _sceDisplayWaitVblankStartCB(CpuThreadState, HandleCallbacks: true);
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
		public int sceDisplaySetFrameBuf(uint Address, int BufferWidth, GuPixelFormats PixelFormat, PspDisplay.SyncMode Sync)
		{
			if (Sync != Core.Display.PspDisplay.SyncMode.Immediate)
			{
				//Console.Error.WriteLine("Not immediate!");
			}
			//Console.WriteLine("sceDisplay.sceDisplaySetFrameBuf {0:X},{1},{2},{3}", Address, BufferWidth, PixelFormat, Sync);
			PspDisplay.CurrentInfo.Address = Address;
			PspDisplay.CurrentInfo.BufferWidth = BufferWidth;
			PspDisplay.CurrentInfo.PixelFormat = PixelFormat;
			PspDisplay.CurrentInfo.Sync = Sync;
			return 0;
		}

		/// <summary>
		/// Wait for vertical blank
		/// </summary>
		[HlePspFunction(NID = 0x36CDFADE, FirmwareVersion = 150)]
		public int sceDisplayWaitVblank(CpuThreadState CpuThreadState)
		{
			return sceDisplayWaitVblankStart(CpuThreadState);
		}

		/// <summary>
		/// Wait for vertical blank with callback
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x8EB9EC49, FirmwareVersion = 150)]
		public int sceDisplayWaitVblankCB(CpuThreadState CpuThreadState)
		{
			// @TODO: Fixme!
			//unimplemented_notice();
			return _sceDisplayWaitVblankStartCB(CpuThreadState, HandleCallbacks: true);
		}

		/// <summary>
		/// Get current HSYNC count
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x773DD3A3, FirmwareVersion = 150)]
		[HlePspNotImplemented(Notice = false)]
		public int sceDisplayGetCurrentHcount()
		{
			// TODO: Properly implement this.

			//PspRtc.Elapsed
			//PspDisplay.cycles_per_pixel
			if (sceDisplayIsVblank())
			{
				return 272;
			}
			else
			{
				return 0;
			}
			//throw(new NotImplementedException());
			//return hleEmulatorState.emulatorState.display.CURRENT_HCOUNT;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://forums.ps2dev.org/viewtopic.php?t=9168"/>
		/// <remarks>(pixel_clk_freq * cycles_per_pixel)/(row_pixels * column_pixel)</remarks>
		/// <returns></returns>
		[HlePspFunction(NID = 0xDBA6C4C4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public float sceDisplayGetFramePerSec()
		{
			// (pixel_clk_freq * cycles_per_pixel)/(row_pixels * column_pixel)
			return 9000000f * 1.0f / (525.0f * 286.0f);
		}

		const float hCountPerVblank = 285.72f;

		/// <summary>
		/// Get accumlated HSYNC count
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x210EAB3A, FirmwareVersion = 150)]
		[HlePspNotImplemented(Notice = false)]
		public int sceDisplayGetAccumulatedHcount()
		{
			return (int)(sceDisplayGetCurrentHcount() + sceDisplayGetVcount() * hCountPerVblank);
		}

		/// <summary>
		/// Number of vertical blank pulses up to now
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x9C6EAAD7, FirmwareVersion = 150)]
		public uint sceDisplayGetVcount()
		{
			return (uint)PspDisplay.VblankCount;
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
		public int sceDisplayGetFrameBuf(ref uint topaddr, ref int bufferwidth, ref GuPixelFormats pixelformat, uint sync)
		{
			topaddr = PspDisplay.CurrentInfo.Address;
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
		/// 
		/// </summary>
		/// <param name="ModeOut"></param>
		/// <param name="WidthOut"></param>
		/// <param name="HeightOut"></param>
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
		/// <param name="CycleCount">Number of VSYNCs to wait before blocking the thread on VBLANK.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x40F1469C, FirmwareVersion = 500, CheckInsideInterrupt = true)]
		public int sceDisplayWaitVblankStartMulti(CpuThreadState CpuThreadState, int CycleCount)
		{
			return _waitVblankCB(CpuThreadState, HandleCallbacks: false, CycleCount: CycleCount);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CycleCount">Number of VSYNCs to wait before blocking the thread on VBLANK.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x77ED8B3A, FirmwareVersion = 500, CheckInsideInterrupt = true)]
		public int sceDisplayWaitVblankStartMultiCB(CpuThreadState CpuThreadState, int CycleCount)
		{
			return _waitVblankCB(CpuThreadState, HandleCallbacks: true, CycleCount: CycleCount);
		}

        [HlePspFunction(NID = 0xB4F378FA, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceDisplayIsForeground()
        {
            return 0;
        }
	}
}
