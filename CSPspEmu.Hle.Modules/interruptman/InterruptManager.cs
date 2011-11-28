using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.interruptman
{
	unsafe public class InterruptManager : HleModuleHost
	{
		//Interrupts.Callback[int][int] handlers;
		//PspCallback[int][int] handlers;

		public enum PspSubInterrupts : uint
		{
		}

		public struct PspIntrHandlerOptionParam
		{
			public int size;
			public uint entry;
			public uint common;
			public uint gp;
			public ushort intr_code;
			public ushort sub_count;
			public ushort intr_level;
			public ushort enabled;
			public uint calls;
			public uint field_1C;
			public uint total_clock_lo;
			public uint total_clock_hi;
			public uint min_clock_lo;
			public uint min_clock_hi;
			public uint max_clock_lo;
			public uint	max_clock_hi;
		}

		/// <summary>
		/// Register a sub interrupt handler.
		/// </summary>
		/// <param name="intno">The interrupt number to register.</param>
		/// <param name="no">The sub interrupt handler number (user controlled) (0-15)</param>
		/// <param name="handler">The interrupt handler</param>
		/// <param name="arg">An argument passed to the interrupt handler</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xCA04A2B9, FirmwareVersion = 150)]
		public int sceKernelRegisterSubIntrHandler(PspSubInterrupts intno, int no, uint handler, uint arg)
		{
			throw (new NotImplementedException());
			/*
			logInfo("sceKernelRegisterSubIntrHandler(%d:%s, %d, %08X, %08X)", intno, to!string(intno), no, handler, arg);
		
			handlers[intno][no] = new PspCallback("sceKernelRegisterSubIntrHandlerCallback", handler, arg);
		
			return 0;
			*/
		}
	
		/*
		CallbacksHandler.Type convertPspSubInterruptsToCallbacksHandlerType(PspSubInterrupts intno) {
			switch (intno) {
				case PspSubInterrupts.PSP_DISPLAY_SUBINT: return CallbacksHandler.Type.VerticalBlank;
				default:
					throw(new Exception("Unhandled convertPspSubInterruptsToCallbacksHandlerType.PspSubInterrupts"));
				break;
			}
		}
		*/

		/// <summary>
		/// Enable a sub interrupt.
		/// </summary>
		/// <param name="intno">The sub interrupt to enable.</param>
		/// <param name="no">The sub interrupt handler number (0-15)</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xFB8E22EC, FirmwareVersion = 150)]
		public int sceKernelEnableSubIntr(PspSubInterrupts intno, int no)
		{
			throw (new NotImplementedException());
			/*
			//cpu.interrupts.registerCallback(cast(Interrupts.Type)intno, handlers[intno][no]);
			//unimplemented();
		
			hleEmulatorState.callbacksHandler.register(
				convertPspSubInterruptsToCallbacksHandlerType(intno),
				handlers[intno][no]
			);

			unimplemented_notice();
			return 0;
			*/
		}

		/// <summary>
		/// Release a sub interrupt handler.
		/// </summary>
		/// <param name="intno">The interrupt number to register.</param>
		/// <param name="no">The sub interrupt handler number (0-15)</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xD61E6961, FirmwareVersion = 150)]
		public int sceKernelReleaseSubIntrHandler(PspSubInterrupts intno, int no)
		{
			throw (new NotImplementedException());
			/*
			hleEmulatorState.callbacksHandler.unregister(
				convertPspSubInterruptsToCallbacksHandlerType(intno),
				handlers[intno][no]
			);

			//cpu.interrupts.unregisterCallback(cast(Interrupts.Type)intno, handlers[intno][no]);
			unimplemented_notice();
			return 0;
			*/
		}

		/// <summary>
		/// Queries the status of a sub interrupt handler.
		/// </summary>
		/// <param name="intno">The interrupt number to register.</param>
		/// <param name="sub_intr_code">?</param>
		/// <param name="data"></param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xD2E8363F, FirmwareVersion = 150)]
		[HlePspFunction(NID = 0x36B1EF81, FirmwareVersion = 150)]
		public int sceKernelQueryIntrHandlerInfo(int intno, int sub_intr_code, PspIntrHandlerOptionParam* data)
		{
			throw (new NotImplementedException());
			/*
			unimplemented();
			return -1;
			*/
		}

	}
}
