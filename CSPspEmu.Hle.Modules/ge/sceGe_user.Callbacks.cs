using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.ge
{
	unsafe public partial class sceGe_user
	{
		/// <summary>
		/// Register callback handlers for the the Ge
		/// </summary>
		/// <param name="PspGeCallbackData">Configured callback data structure</param>
		/// <returns>The callback ID, less than 0 on error</returns>
		[HlePspFunction(NID = 0xA4FC06A4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeSetCallback(PspGeCallbackData* PspGeCallbackData)
		{
			return 1;
		}

		/// <summary>
		/// Unregister the callback handlers
		/// </summary>
		/// <param name="cbid">The ID of the callbacks from sceGeSetCallback</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x05DB22CE, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeUnsetCallback(int cbid)
		{
			return 0;
		}

		/// <summary>
		/// alias void function(int id, void *arg) PspGeCallback;
		/// </summary>
		public enum PspGeCallback : uint { }

		/// <summary>
		/// 
		/// </summary>
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
	}
}
