using System;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core;
using CSPspEmu.Core.Rtc;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.threadman
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class ThreadManForUser : HleModuleHost
	{
		[Inject]
		PspRtc PspRtc;

		static Logger Logger = Logger.GetLogger("ThreadManForUser");

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
			PspRtc.Update();
			return PspRtc.ElapsedTime.TotalMicroseconds;
			//return Platform.CurrentUnixMicroseconds;
		}

		/// <summary>
		/// Convert a number of microseconds to a ::SceKernelSysClock structure
		/// </summary>
		/// <param name="MicroSeconds">Number of microseconds</param>
		/// <param name="Clock">Pointer to a <see cref="SceKernelSysClock"/> structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x110DEC9A, FirmwareVersion = 150)]
		public int sceKernelUSec2SysClock(uint MicroSeconds, SceKernelSysClock* Clock)
		{
			Clock->MicroSeconds = MicroSeconds;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Clock"></param>
		/// <param name="Low"></param>
		/// <param name="High"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE1619D7C, FirmwareVersion = 150)]
		public int sceKernelSysClock2USecWide(long Clock, uint* Low, uint* High)
		{
			if (Low != null) *Low = (uint)(Clock % 1000000);
			if (High != null) *High = (uint)(Clock / 1000000);
			return 0;
		}

		/// <summary>
		/// Set an alarm.
		/// </summary>
		/// <param name="clock">The number of micro seconds till the alarm occurrs.</param>
		/// <param name="handler">Pointer to a SceKernelAlarmHandler</param>
		/// <param name="common">Common pointer for the alarm handler</param>
		/// <returns>
		///		A UID representing the created alarm
		///		less than 0 on error.
		/// </returns>
		[HlePspFunction(NID = 0x6652B8CA, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelSetAlarm(int clock, /*SceKernelAlarmHandler*/uint handler, void* common)
		{
			//throw(new NotImplementedException());
			return -1;
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
		/// <param name="Time">Pointer to a <see cref="SceKernelSysClock"/> structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xDB738F35, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceKernelGetSystemTime(SceKernelSysClock* Time)
		{
			//Console.Error.WriteLine(sceKernelGetSystemTimeWide());
			Time->MicroSeconds = sceKernelGetSystemTimeWide();
			return 0;
		}

		/// <summary>
		/// Convert a <see cref="SceKernelSysClock"/> structure to microseconds
		/// </summary>
		/// <param name="Clock">Pointer to a <see cref="SceKernelSysClock"/> structure</param>
		/// <param name="Low">Pointer to the low part of the time</param>
		/// <param name="High">Pointer to the high part of the time</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xBA6B92E2, FirmwareVersion = 150)]
		public int sceKernelSysClock2USec(SceKernelSysClock* Clock, uint* Low, uint* High)
		{
			return sceKernelSysClock2USecWide(Clock->MicroSeconds, Low, High);
		}

		/// <summary>
		/// Convert a number of microseconds to a wide time
		/// </summary>
		/// <param name="MicroSeconds">Number of microseconds.</param>
		/// <returns>The time</returns>
		[HlePspFunction(NID = 0xC8CD158C, FirmwareVersion = 150)]
		public long sceKernelUSec2SysClockWide(uint MicroSeconds)
		{
			return MicroSeconds;
		}

		[HlePspFunction(NID = 0x64D4540E, FirmwareVersion = 150)]
		public long sceKernelReferThreadProfiler()
		{
			//Valid only on actual hardware with debug mode enabled.
			return 0;
		}

		[HlePspFunction(NID = 0xFFC36A14, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelReferThreadRunStatus()
		{
			return 0;
		}

		/**
		 * Delay the current thread by a specified number of sysclocks
		 *
		 * @param sysclocksPointer - Address of delay in sysclocks
		 *
		 * @return 0 on success, < 0 on error
		 */
		[HlePspFunction(NID = 0xBD123D9E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelDelaySysClockThread()
		{
			return 0;
		}
	}
}
