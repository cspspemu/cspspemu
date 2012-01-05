using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.threadman
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
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
		/// Convert a number of microseconds to a ::SceKernelSysClock structure
		/// </summary>
		/// <param name="usec">Number of microseconds</param>
		/// <param name="clock">Pointer to a ::SceKernelSysClock structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x110DEC9A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelUSec2SysClock(uint usec, SceKernelSysClock* clock)
		{
			throw(new NotImplementedException());
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

		/// <summary>
		/// Get the system time
		/// </summary>
		/// <param name="Time">Pointer to a ::SceKernelSysClock structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xDB738F35, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelGetSystemTime(SceKernelSysClock* Time)
		{
			Time->MicroSeconds = sceKernelGetSystemTimeWide();
			return 0;
		}

		/// <summary>
		/// Convert a ::SceKernelSysClock structure to microseconds
		/// </summary>
		/// <param name="Clock">Pointer to a ::SceKernelSysClock structure</param>
		/// <param name="Low">Pointer to the low part of the time</param>
		/// <param name="High">Pointer to the high part of the time</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xBA6B92E2, FirmwareVersion = 150)]
		public int sceKernelSysClock2USec(SceKernelSysClock* Clock, uint* Low, uint* High)
		{
			*Low = Clock->Low;
			*High = Clock->High;
			return 0;
		}
	}
}
