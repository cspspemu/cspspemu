using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.ge
{
	unsafe public class sceGe_user : HleModuleHost
	{
		// alias void function(int id, void *arg) PspGeCallback;
		public enum PspGeCallback : uint { }

		public struct PspGeCallbackData
		{
			/// <summary>
			/// GE callback for the signal interrupt
			/// </summary>
			public PspGeCallback SignalFunction;

			/// <summary>
			/// GE callback argument for signal interrupt
			/// </summary>
			public uint SignalArgument;

			/// <summary>
			/// GE callback for the finish interrupt
			/// </summary>
			public PspGeCallback FinishFunction;
			
			/// <summary>
			/// GE callback argument for finish interrupt
			/// </summary>
			public uint FinishArgument;
		}

		/// <summary>
		/// Get the address of VRAM.
		/// </summary>
		/// <returns>A pointer to the base of VRAM.</returns>
		[HlePspFunction(NID = 0xE47E40E4, FirmwareVersion = 150)]
		public uint sceGeEdramGetAddr()
		{
			return HleState.Processor.Memory.FrameBufferSegment.Low;
		}

		/// <summary>
		/// Register callback handlers for the the Ge
		/// </summary>
		/// <param name="PspGeCallbackData">Configured callback data structure</param>
		/// <returns>The callback ID, less than 0 on error</returns>
		[HlePspFunction(NID = 0xA4FC06A4, FirmwareVersion = 150)]
		int sceGeSetCallback(PspGeCallbackData* PspGeCallbackData)
		{
			/*
			int n;
			PspGeCallbackData* callbackDataPtr;
			for (n = 0; n < callbackDataList.length; n++) {
				callbackDataPtr = &callbackDataList[n];
				if (*callbackDataPtr == PspGeCallbackData.init) {
					*callbackDataPtr = *cb;
					break;
				}
			}

			if (n == callbackDataList.length) {
				callbackDataList ~= *cb;
				callbackDataPtr = &callbackDataList[$ - 1];
			}
		
			return cast(int)cast(void *)callbackDataPtr;
			*/
			HleState.CallbackManager.Callbacks.Create(new HleCallback()
			{
				Function = (uint)PspGeCallbackData[0].FinishFunction,
				Argument = PspGeCallbackData[0].FinishArgument,
			});
			return 1;
		}
	}
}
