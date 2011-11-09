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


		/// <summary>
		/// Set the controller mode.
		/// </summary>
		/// <param name="mode">
		/// One of ::PspCtrlMode.
		/// PSP_CTRL_MODE_DIGITAL = 0
		/// PSP_CTRL_MODE_ANALOG  = 1
		/// 
		/// PSP_CTRL_MODE_DIGITAL is the same as PSP_CTRL_MODE_ANALOG
		/// except that doesn't update Lx and Ly values. Setting them to 0x80.
		/// </param>
		/// <returns>The previous mode.</returns>
		[HlePspFunction(NID = 0x1F4011E6, FirmwareVersion = 150)]
		public int sceCtrlSetSamplingMode(int mode)
		{
			/*
			uint previouseMode = cast(int)cpu.controller.samplingMode;
			cpu.controller.samplingMode = cast(Controller.Mode)mode;
			return previouseMode;
			*/
			return 0;
		}

		/// <summary>
		/// Set the controller cycle setting.
		/// </summary>
		/// <param name="cycle">
		/// Cycle. Normally set to 0.
		/// 
		/// @TODO Unknown what this means exactly.
		/// </param>
		/// <returns>The previous cycle setting.</returns>
		[HlePspFunction(NID = 0x6A2774F3, FirmwareVersion = 150)]
		public int sceCtrlSetSamplingCycle(int cycle)
		{
			/*
			int previousCycle = cpu.controller.samplingCycle;
			cpu.controller.samplingCycle = cycle;
			if (cycle != 0) writefln("sceCtrlSetSamplingCycle != 0! :: %d", cycle);
			return previousCycle;
			*/
			return 0;
		}
	}
}
