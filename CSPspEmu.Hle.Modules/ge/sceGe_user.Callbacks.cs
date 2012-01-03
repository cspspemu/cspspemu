using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu;

namespace CSPspEmu.Hle.Modules.ge
{
	unsafe public partial class sceGe_user
	{
		int CallbackLastId = 1;
		public Dictionary<int, PspGeCallbackData> Callbacks = new Dictionary<int, PspGeCallbackData>();
		//PspGeCallbackData

		/// <summary>
		/// Register callback handlers for the the Ge
		/// </summary>
		/// <param name="PspGeCallbackData">Configured callback data structure</param>
		/// <returns>The callback ID, less than 0 on error</returns>
		[HlePspFunction(NID = 0xA4FC06A4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeSetCallback(PspGeCallbackData* PspGeCallbackData)
		{
			int CallbackId = CallbackLastId++;
			Callbacks[CallbackId] = *PspGeCallbackData;
			//Console.Error.WriteLine("{0}", *PspGeCallbackData);
			return CallbackId;
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
			Callbacks.Remove(cbid);
			return 0;
		}
	}
}
