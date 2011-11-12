using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.rtc
{
	unsafe public class sceRtc : HleModuleHost
	{
		/// <summary>
		/// Get the resolution of the tick counter
		/// </summary>
		/// <returns>Number of ticks per second</returns>
		[HlePspFunction(NID = 0xC41C2853, FirmwareVersion = 150)]
		public uint sceRtcGetTickResolution()
		{
			return (uint)(TimeSpan.FromSeconds(1).TotalMilliseconds * 1000);
		}

		/// <summary>
		/// Get current tick count (number of microseconds)
		/// </summary>
		/// <param name="Tick">pointer to u64 to receive tick count</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x3F7AD767, FirmwareVersion = 150)]
		public int sceRtcGetCurrentTick(ulong* Tick)
		{
			HleState.PspRtc.Update();
			*Tick = (ulong)(HleState.PspRtc.Elapsed.TotalMilliseconds * 1000);
			return 0;
		}

	}
}
