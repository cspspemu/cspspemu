using CSPspEmu.Core.Types;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Controller
{
	public class PspController
	{
		public const int MaxStoredFrames = 128;

		public enum SamplingModeEnum
		{
			Digital = 0,
			Analogic = 1,
		}

		protected List<SceCtrlData> SceCtrlDataBuffer = new List<SceCtrlData>();
		public int SamplingCycle;
		public SamplingModeEnum SamplingMode;
		public int LatchSamplingCount;
		public uint LastTimestamp;

		public PspController()
		{
			for (int n = 0; n < MaxStoredFrames; n++)
			{
				InsertSceCtrlData(default(SceCtrlData));
			}
		}

		public void InsertSceCtrlData(SceCtrlData SceCtrlData)
		{
			lock (this)
			{
				SceCtrlData.TimeStamp = LastTimestamp++;
				SceCtrlDataBuffer.Add(SceCtrlData);
				if (SceCtrlDataBuffer.Count > MaxStoredFrames) SceCtrlDataBuffer.RemoveAt(0);
			}
			LatchSamplingCount++;
		}

		public SceCtrlData GetSceCtrlDataAt(int Index)
		{
			lock (this)
			{
				return SceCtrlDataBuffer[SceCtrlDataBuffer.Count - Index - 1];
			}
		}
	}
}
