using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public struct SceKernelVTimerOptParam
		{
			public uint StructSize;
		}

		/// <summary>
		/// Create a virtual timer
		/// </summary>
		/// <param name="Name">Name for the timer.</param>
		/// <param name="SceKernelVTimerOptParam">Pointer to an ::SceKernelVTimerOptParam (pass NULL)</param>
		/// <returns>The VTimer's UID or less than 0 on error.</returns>
		[HlePspFunction(NID = 0x20FFF560, FirmwareVersion = 150)]
		public int sceKernelCreateVTimer(string Name, SceKernelVTimerOptParam *SceKernelVTimerOptParam)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Cancel the timer handler
		/// </summary>
		/// <param name="VTimerId">The UID of the vtimer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD2D615EF, FirmwareVersion = 150)]
		public int sceKernelCancelVTimerHandler(int VTimerId)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Start a virtual timer
		/// </summary>
		/// <param name="VTimerId">The UID of the timer</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xC68D9437, FirmwareVersion = 150)]
		public int sceKernelStartVTimer(int VTimerId)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Stop a virtual timer
		/// </summary>
		/// <param name="VTimerId">The UID of the timer</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xD0AEEE87, FirmwareVersion = 150)]
		public int sceKernelStopVTimer(int VTimerId)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get the timer time
		/// </summary>
		/// <param name="VTimerId">UID of the vtimer</param>
		/// <param name="Time">Pointer to a ::SceKernelSysClock structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x034A921F, FirmwareVersion = 150)]
		public int sceKernelGetVTimerTime(int VTimerId, SceKernelSysClock* Time)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get the timer time (wide format)
		/// </summary>
		/// <param name="VTimerId">UID of the vtimer</param>
		/// <returns>The 64bit timer time</returns>
		[HlePspFunction(NID = 0xC0B3FFD2, FirmwareVersion = 150)]
		public long sceKernelGetVTimerTimeWide(int VTimerId)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Set the timer handler.
		/// 
		/// Timer handler will be executed once after
		/// </summary>
		/// <param name="VTimerId">UID of the vtimer</param>
		/// <param name="Time">Time to call the handler?</param>
		/// <param name="Handler">The timer handler</param>
		/// <param name="CommonPointer">Common pointer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD8B299AE, FirmwareVersion = 150)]
		public int sceKernelSetVTimerHandler(int VTimerId, SceKernelSysClock *Time, uint Handler, uint CommonPointer)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Set the timer handler (wide mode)
		/// </summary>
		/// <param name="VTimerId">UID of the vtimer</param>
		/// <param name="Time">Time to call the handler?</param>
		/// <param name="Handler">The timer handler</param>
		/// <param name="CommonPointer">Common pointer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		public int sceKernelSetVTimerHandlerWide(int VTimerId, long Time, uint Handler, uint CommonPointer)
		{
			throw (new NotImplementedException());
		}
	}
}
