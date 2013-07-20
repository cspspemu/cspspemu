using System;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Hle.Managers;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.threadman
{
	public unsafe partial class ThreadManForUser
	{
		public struct SceKernelVTimerOptParam
		{
			public uint StructSize;
		}

		/// <summary>
		/// Create a virtual timer
		/// </summary>
		/// <param name="Name">Name for the timer.</param>
		/// <param name="SceKernelVTimerOptParam">Pointer to an <see cref="SceKernelVTimerOptParam"/> (pass NULL)</param>
		/// <returns>The VTimer's UID or less than 0 on error.</returns>
		[HlePspFunction(NID = 0x20FFF560, FirmwareVersion = 150)]
		public VirtualTimer sceKernelCreateVTimer(string Name, SceKernelVTimerOptParam* SceKernelVTimerOptParam)
		{
			var VirtualTimer = new VirtualTimer(InjectContext, Name);
			if (SceKernelVTimerOptParam != null) VirtualTimer.SceKernelVTimerOptParam = *SceKernelVTimerOptParam;
			VirtualTimer.Id = VirtualTimer.GetUidIndex(InjectContext);
			return VirtualTimer;
		}

		/// <summary>
		/// Start a virtual timer
		/// </summary>
		/// <param name="VirtualTimer">The UID of the timer</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xC68D9437, FirmwareVersion = 150)]
		public int sceKernelStartVTimer(VirtualTimer VirtualTimer)
		{
			VirtualTimer.Start();
			return 0;
		}

		/// <summary>
		/// Stop a virtual timer
		/// </summary>
		/// <param name="VirtualTimer">The UID of the timer</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xD0AEEE87, FirmwareVersion = 150)]
		public int sceKernelStopVTimer(VirtualTimer VirtualTimer)
		{
			VirtualTimer.Stop();
			return 0;
		}

		/// <summary>
		/// Set the timer time (wide format)
		/// </summary>
		/// <param name="VirtualTimer">UID of the vtimer</param>
		/// <param name="Time">A <see cref="SceKernelSysClock"/> structure</param>
		/// <returns>Possibly the last time</returns>
		[HlePspFunction(NID = 0xFB6425C3, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		[PspUntested]
		public int sceKernelSetVTimerTimeWide(VirtualTimer VirtualTimer, long Time)
		{
			throw(new NotImplementedException());
			//VirtualTimer.ElapsedMicroseconds = Time;
			return 0;
		}

		/// <summary>
		/// Get the timer time
		/// </summary>
		/// <param name="VirtualTimer">UID of the vtimer</param>
		/// <param name="Time">Pointer to a <see cref="SceKernelSysClock"/> structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x034A921F, FirmwareVersion = 150)]
		public int sceKernelGetVTimerTime(VirtualTimer VirtualTimer, SceKernelSysClock* Time)
		{
			return (int)VirtualTimer.ElapsedMicroseconds;
		}

		/// <summary>
		/// Get the timer time (wide format)
		/// </summary>
		/// <param name="VirtualTimer">UID of the vtimer</param>
		/// <returns>The 64bit timer time</returns>
		[HlePspFunction(NID = 0xC0B3FFD2, FirmwareVersion = 150)]
		public long sceKernelGetVTimerTimeWide(VirtualTimer VirtualTimer)
		{
			return VirtualTimer.ElapsedMicroseconds;
		}

		/// <summary>
		/// Set the timer handler.
		/// 
		/// Timer handler will be executed once after
		/// </summary>
		/// <param name="VirtualTimer">UID of the vtimer</param>
		/// <param name="Time">Time to call the handler?</param>
		/// <param name="HandlerCallback">The timer handler</param>
		/// <param name="HandlerArgument">Common pointer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD8B299AE, FirmwareVersion = 150)]
		public int sceKernelSetVTimerHandler(VirtualTimer VirtualTimer, SceKernelSysClock* Time, PspPointer HandlerCallback, PspPointer HandlerArgument)
		{
			VirtualTimer.SetHandler(Time: Time->MicroSeconds, HandlerCallback: HandlerCallback, HandlerArgument: HandlerArgument, HandlerIsWide: false);
			return 0;
		}

		/// <summary>
		/// Set the timer handler (wide mode)
		/// </summary>
		/// <param name="VirtualTimer">UID of the vtimer</param>
		/// <param name="Time">Time to call the handler?</param>
		/// <param name="HandlerCallback">The timer handler</param>
		/// <param name="HandlerArgument">Common pointer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x53B00E9A, FirmwareVersion = 150)]
		public int sceKernelSetVTimerHandlerWide(VirtualTimer VirtualTimer, long Time, PspPointer HandlerCallback, PspPointer HandlerArgument)
		{
			VirtualTimer.SetHandler(Time: Time, HandlerCallback: HandlerCallback, HandlerArgument: HandlerArgument, HandlerIsWide: true);
			return 0;
		}

		/// <summary>
		/// Cancel the timer handler
		/// </summary>
		/// <param name="VirtualTimer">The UID of the vtimer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD2D615EF, FirmwareVersion = 150)]
		public int sceKernelCancelVTimerHandler(VirtualTimer VirtualTimer)
		{
			VirtualTimer.CancelHandler();
			return 0;
		}

		/// <summary>
		/// Deletes the timer handler
		/// </summary>
		/// <param name="VirtualTimer">The UID of the vtimer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x328F9E52, FirmwareVersion = 150)]
		public int sceKernelDeleteVTimer(VirtualTimer VirtualTimer)
		{
			VirtualTimer.RemoveUid(InjectContext);
			return 0;
		}
	}
}
