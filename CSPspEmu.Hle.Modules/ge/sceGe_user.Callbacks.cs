using System.Collections.Generic;
using CSPspEmu.Core.Gpu;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.ge
{
    public partial class sceGe_user
	{
		static Logger Logger = Logger.GetLogger("sceGe");

		int CallbackLastId = 1;
		public Dictionary<int, PspGeCallbackData> Callbacks = new Dictionary<int, PspGeCallbackData>();
		//PspGeCallbackData

		/// <summary>
		/// Register callback handlers for the the Ge
		/// </summary>
		/// <param name="PspGeCallbackData">Configured callback data structure</param>
		/// <returns>The callback ID, less than 0 on error</returns>
		[HlePspFunction(NID = 0xA4FC06A4, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceGeSetCallback(ref PspGeCallbackData PspGeCallbackData)
		{
			int CallbackId = CallbackLastId++;
			Callbacks[CallbackId] = PspGeCallbackData;

			var CallbackData = PspGeCallbackData;

			/*
			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Cyan, () =>
			{
				Console.WriteLine("PspGeCallbackData.Finish(0x{0:X}) : (0x{1:X})", CallbackData.FinishFunction, CallbackData.FinishArgument);
				Console.WriteLine("PspGeCallbackData.Signal(0x{0:X}) : (0x{1:X})", CallbackData.SignalFunction, CallbackData.SignalArgument);
			});
			*/
			
			Logger.Info("PspGeCallbackData.Finish(0x{0:X}) : (0x{1:X})", PspGeCallbackData.FinishFunction, PspGeCallbackData.FinishArgument);
			Logger.Info("PspGeCallbackData.Signal(0x{0:X}) : (0x{1:X})", PspGeCallbackData.SignalFunction, PspGeCallbackData.SignalArgument);

			//Console.Error.WriteLine("{0}", *PspGeCallbackData);
			return CallbackId;
		}

		/// <summary>
		/// Unregister the callback handlers
		/// </summary>
		/// <param name="cbid">The ID of the callbacks from sceGeSetCallback</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x05DB22CE, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceGeUnsetCallback(int cbid)
		{
			Callbacks.Remove(cbid);
			return 0;
		}
	}
}
