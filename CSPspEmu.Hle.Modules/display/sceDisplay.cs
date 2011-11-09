using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.display
{
	public class sceDisplay : HleModuleHost
	{
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
			Console.WriteLine("sceDisplay.sceDisplaySetMode");

			HleState.PspDisplay.CurrentInfo.Mode = Mode;
			HleState.PspDisplay.CurrentInfo.Width = Width;
			HleState.PspDisplay.CurrentInfo.Height = Height;
			return 0;
		}

		/// <summary>
		/// Wait for vertical blank start
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x984C27E7, FirmwareVersion = 150)]
		public int sceDisplayWaitVblankStart(CpuThreadState CpuThreadState)
		{
			if (HleState.PspDisplay.Vsync)
			{
				var SleepThread = HleState.ThreadManager.Current;
				SleepThread.CurrentStatus = HleThread.Status.Waiting;
				SleepThread.CurrentWaitType = HleThread.WaitType.Timer;
				SleepThread.AwakeOnTime = HleState.PspRtc.CurrentDateTime + TimeSpan.FromMilliseconds(1000 / 60);
			}
			CpuThreadState.Yield();

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
		public int sceDisplaySetFrameBuf(uint Address, int BufferWidth, PspDisplay.PixelFormats PixelFormat, PspDisplay.SyncMode Sync)
		{
			Console.WriteLine("sceDisplay.sceDisplaySetFrameBuf {0:X},{1},{2},{3}", Address, BufferWidth, PixelFormat, Sync);
			HleState.PspDisplay.CurrentInfo.Address = Address;
			HleState.PspDisplay.CurrentInfo.BufferWidth = BufferWidth;
			HleState.PspDisplay.CurrentInfo.PixelFormat = PixelFormat;
			HleState.PspDisplay.CurrentInfo.Sync = Sync;
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
	}
}
