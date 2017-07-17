using System;
using System.Collections.Generic;
using CSharpUtils;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Components.Controller
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
            ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
            {
                //Console.WriteLine("PspController");
            });
            for (var n = 0; n < MaxStoredFrames; n++)
            {
                InsertSceCtrlData(default(SceCtrlData).Init());
            }
        }

        public void InsertSceCtrlData(SceCtrlData sceCtrlData)
        {
            lock (this)
            {
                sceCtrlData.TimeStamp = LastTimestamp++;
                SceCtrlDataBuffer.Add(sceCtrlData);
                if (SceCtrlDataBuffer.Count > MaxStoredFrames) SceCtrlDataBuffer.RemoveAt(0);
            }
            LatchSamplingCount++;
        }

        public SceCtrlData GetSceCtrlDataAt(int index)
        {
            lock (this)
            {
                return SceCtrlDataBuffer[SceCtrlDataBuffer.Count - index - 1];
            }
        }
    }
}