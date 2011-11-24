﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSPspEmu.Core;
using CSPspEmu.Core.Controller;

namespace CSPspEmu.Hle.Modules.ctrl
{
	unsafe public class sceCtrl : HleModuleHost
	{
		protected void _ReadCount(SceCtrlData* SceCtrlData, int Count, bool Peek, bool Positive)
		{
			for (int n = 0; n < Count; n++)
			{
				SceCtrlData[n] = HleState.PspController.GetSceCtrlDataAt(n);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SceCtrlData"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x3A622550, FirmwareVersion = 150)]
		public int sceCtrlPeekBufferPositive(SceCtrlData* SceCtrlData, int Count) {
			_ReadCount(SceCtrlData, Count, Peek: true, Positive: true);
			return Count;
		}

		// sceCtrlReadBufferPositive () is blocking and waits for vblank (slower).
		/// <summary>
		/// Read buffer positive
		/// </summary>
		/// <example>
		///		SceCtrlData pad;
		///		
		///		sceCtrlSetSamplingCycle(0);
		///		sceCtrlSetSamplingMode(1);
		///		sceCtrlReadBufferPositive(&pad, 1);
		///		// Do something with the read controller data
		/// </example>
		/// <param name="pad_data">Pointer to a ::SceCtrlData structure used hold the returned pad data.</param>
		/// <param name="count">Number of ::SceCtrlData buffers to read.</param>
		/// <returns>Count?</returns>
		[HlePspFunction(NID = 0x1F803938, FirmwareVersion = 150, SkipLog = true)]
		public int sceCtrlReadBufferPositive(SceCtrlData* SceCtrlData, int Count)
		{
			_ReadCount(SceCtrlData, Count, Peek: false, Positive: true);
			return Count;
		}

		/// <summary>
		/// Set the controller mode.
		/// </summary>
		/// <param name="SamplingMode">
		/// One of ::PspCtrlMode.
		/// PSP_CTRL_MODE_DIGITAL = 0
		/// PSP_CTRL_MODE_ANALOG  = 1
		/// 
		/// PSP_CTRL_MODE_DIGITAL is the same as PSP_CTRL_MODE_ANALOG
		/// except that doesn't update Lx and Ly values. Setting them to 0x80.
		/// </param>
		/// <returns>The previous mode.</returns>
		[HlePspFunction(NID = 0x1F4011E6, FirmwareVersion = 150)]
		public PspController.SamplingModeEnum sceCtrlSetSamplingMode(PspController.SamplingModeEnum SamplingMode)
		{
			try
			{
				return HleState.PspController.SamplingMode;
			}
			finally
			{
				HleState.PspController.SamplingMode = SamplingMode;
			}
		}

		/// <summary>
		/// Set the controller cycle setting.
		/// </summary>
		/// <param name="SamplingCycle">
		/// Cycle. Normally set to 0.
		/// 
		/// @TODO Unknown what this means exactly.
		/// </param>
		/// <returns>The previous cycle setting.</returns>
		[HlePspFunction(NID = 0x6A2774F3, FirmwareVersion = 150)]
		public int sceCtrlSetSamplingCycle(int SamplingCycle)
		{
			try
			{
				return HleState.PspController.SamplingCycle;
			}
			finally
			{
				HleState.PspController.SamplingCycle = SamplingCycle;
			}
		}
	}
}
