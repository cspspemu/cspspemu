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

namespace CSPspEmu.Hle.Modules.display
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public class sceDisplay : HleModuleHost
	{
		protected PspDisplay PspDisplay { get { return HleState.PspDisplay; } }
		protected PspRtc PspRtc { get { return HleState.PspRtc; } }
		protected PspConfig PspConfig { get { return HleState.PspConfig; } }
		protected HleThreadManager ThreadManager { get { return HleState.ThreadManager; } }

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
		DateTime LastWaitVblankStart;

		private int _sceDisplayWaitVblankStartCB(CpuThreadState CpuThreadState, bool HandleCallbacks)
		{
			if (PspConfig.VerticalSynchronization && LastVblankCount != PspDisplay.VblankCount)
			{
				var SleepThread = ThreadManager.Current;

				SleepThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Display, "sceDisplayWaitVblankStart", (WakeUpCallbackDelegate) =>
				{
					PspRtc.RegisterTimerAtOnce(LastWaitVblankStart + TimeSpan.FromMilliseconds(1000 / 60), () =>
					{
						WakeUpCallbackDelegate();
					});
					LastWaitVblankStart = PspRtc.UpdatedCurrentDateTime;
				}, HandleCallbacks: HandleCallbacks);

				/*
				SleepThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Display, "sceDisplayWaitVblankStart", (WakeUpCallbackDelegate) =>
				{
					//PspDisplay.VBlankEvent
					HleState.PspDisplay.VBlankEvent.CallbackOnStateOnce(() =>
					{
						LastVblankCount = HleState.PspDisplay.VblankCount;
						WakeUpCallbackDelegate();
					});
				});
				*/
			}

			return 0;
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
		//[HlePspNotImplemented]
		public int sceDisplayGetCurrentHcount()
		{
			//PspRtc.Elapsed
			//PspDisplay.cycles_per_pixel

			return 0;
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

		/// <summary>
		/// Get accumlated HSYNC count
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x210EAB3A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceDisplayGetAccumulatedHcount()
		{
			return 0;
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
		public int sceDisplayGetFrameBuf(uint* topaddr, int* bufferwidth, GuPixelFormats* pixelformat, uint sync)
		{
			*topaddr = HleState.PspDisplay.CurrentInfo.Address;
			*bufferwidth = HleState.PspDisplay.CurrentInfo.BufferWidth;
			*pixelformat = HleState.PspDisplay.CurrentInfo.PixelFormat;
			return 0;
		}
	}
}
