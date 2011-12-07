using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser : HleModuleHost
	{
		/*
		
		public uint Test(CpuThreadState CpuThreadState, int a, int b, string c)
		{
			//Console.WriteLine(CpuThreadState);
			return (uint)a + (uint)b;
		}
		*/

		/// <summary>
		/// Get the low 32bits of the current system time (microseconds)
		/// </summary>
		/// <returns>The low 32bits of the system time</returns>
		/// 
		[HlePspFunction(NID = 0x369ED59D, FirmwareVersion = 150)]
		public uint sceKernelGetSystemTimeLow()
		{
			return (uint)sceKernelGetSystemTimeWide();
		}

		/// <summary>
		/// Get the system time (wide version)
		/// </summary>
		/// <returns>The system time</returns>
		[HlePspFunction(NID = 0x82BC5777, FirmwareVersion = 150)]
		public long sceKernelGetSystemTimeWide()
		{
			HleState.PspRtc.Update();
			return (long)(HleState.PspRtc.Elapsed.TotalMilliseconds * 1000);
		}

		/// <summary>
		/// Set an alarm.
		/// </summary>
		/// <param name="clock">The number of micro seconds till the alarm occurrs.</param>
		/// <param name="handler">Pointer to a ::SceKernelAlarmHandler</param>
		/// <param name="common">Common pointer for the alarm handler</param>
		/// <returns>
		///		A UID representing the created alarm
		///		less than 0 on error.
		/// </returns>
		[HlePspFunction(NID = 0x6652B8CA, FirmwareVersion = 150)]
		public int sceKernelSetAlarm(int clock, /*SceKernelAlarmHandler*/uint handler, void* common)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Cancel a pending alarm.
		/// </summary>
		/// <param name="alarmid">UID of the alarm to cancel.</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x7E65B999, FirmwareVersion = 150)]
		public int sceKernelCancelAlarm(int alarmid)
		{
			throw(new NotImplementedException());
		}
	}
}
