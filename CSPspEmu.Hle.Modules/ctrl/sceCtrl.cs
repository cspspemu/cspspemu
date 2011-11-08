using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Modules.ctrl
{
	unsafe public class sceCtrl : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="SceCtrlData"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x3A622550, FirmwareVersion = 150)]
		public int sceCtrlPeekBufferPositive(SceCtrlData* SceCtrlData, int Count) {
			//SceCtrlData[0].Buttons = 0xFFFFFFFF;
			//throw(new NotImplementedException());
			SceCtrlData[0] = HleState.PspController.SceCtrlData;
			return 0;
			//readBufferedFrames(pad_data, count, true);
			//return count;
		}

	}
}
