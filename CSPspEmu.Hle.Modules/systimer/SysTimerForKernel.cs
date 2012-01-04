using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.systimer
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public class SysTimerForKernel : HleModuleHost
	{
		public enum SceSysTimerId : int { }

		/// <summary>
		/// Allocate a new SysTimer timer instance.
		/// </summary>
		/// <returns>SysTimerId on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xC99073E3, FirmwareVersion = 150)]
		public SceSysTimerId sceSTimerAlloc()
		{
			/*
			SceSysTimerId sysTimerId = uniqueIdFactory.add!SysTimer(new SysTimer(hleEmulatorState, currentThreadState));
			logInfo("sceSTimerAlloc() : %d", sysTimerId);
			return sysTimerId;
			*/
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Setup a SysTimer handler
		/// </summary>
		/// <param name="SysTimerId">The timer id.</param>
		/// <param name="Cycle">The timer cycle in microseconds (???). Maximum: 4194303 which represents ~1/10 seconds.</param>
		/// <param name="Handler">The handler function. Has to return -1.</param>
		/// <param name="Unknown">Unknown. Pass 0.</param>
		[HlePspFunction(NID = 0x975D8E84, FirmwareVersion = 150)]
		public void sceSTimerSetHandler(SceSysTimerId SysTimerId, int Cycle, uint Handler, int Unknown)
		{
			/*
			logInfo("sceSTimerSetHandler(%d, %d, %08X, %d)", SysTimerId, Cycle, Handler, Unknown);
			SysTimer sysTimer = uniqueIdFactory.get!SysTimer(SysTimerId);
			sysTimer.setHandler(Cycle, Handler, Unknown);
			*/
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Start the SysTimer timer count.
		/// </summary>
		/// <param name="SysTimerId">The timer id.</param>
		[HlePspFunction(NID = 0xA95143E2, FirmwareVersion = 150)]
		public void sceSTimerStartCount(SceSysTimerId SysTimerId)
		{
			/*
			logInfo("sceSTimerStartCount(%d)", SysTimerId);
			uniqueIdFactory.get!SysTimer(SysTimerId).start();
			*/
			throw (new NotImplementedException());
		}
	}
}
