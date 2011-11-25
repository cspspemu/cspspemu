using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.display
{
	public class sceDisplay : HleModuleHost
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

		/// <summary>
		/// Wait for vertical blank start
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x984C27E7, FirmwareVersion = 150)]
		public int sceDisplayWaitVblankStart(CpuThreadState CpuThreadState)
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
				});

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
		/// Display set framebuf
		/// </summary>
		/// <param name="Address">Address of start of framebuffer</param>
		/// <param name="BufferWidth">buffer width (must be power of 2)</param>
		/// <param name="PixelFormat">One of ::PspDisplayPixelFormats.</param>
		/// <param name="Sync">One of ::PspDisplaySetBufSync</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x289D82FE, FirmwareVersion = 150)]
		public int sceDisplaySetFrameBuf(uint Address, int BufferWidth, PixelFormats PixelFormat, PspDisplay.SyncMode Sync)
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
		/// Number of vertical blank pulses up to now
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x9C6EAAD7, FirmwareVersion = 150)]
		public uint sceDisplayGetVcount()
		{
			return (uint)PspDisplay.VblankCount;
		}
	
	}
}
