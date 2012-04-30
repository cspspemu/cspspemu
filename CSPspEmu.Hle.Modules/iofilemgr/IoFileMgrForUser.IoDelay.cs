using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public partial class IoFileMgrForUser
	{
		[Inject]
		PspRtc PspRtc;

		[Inject]
		HleThreadManager ThreadManager;

		private class IoDelayType
		{
			static public readonly IoDelayType Open = new IoDelayType("Open", TimeSpan.FromMilliseconds(5));
			static public readonly IoDelayType Close = new IoDelayType("Close", TimeSpan.FromMilliseconds(1));
			static public readonly IoDelayType Seek = new IoDelayType("Seek", TimeSpan.FromMilliseconds(0));
			static public readonly IoDelayType Ioctl = new IoDelayType("Ioctl", TimeSpan.FromMilliseconds(2));
			static public readonly IoDelayType Devctl = new IoDelayType("Devctl", TimeSpan.FromMilliseconds(2));
			static public readonly IoDelayType Remove = new IoDelayType("Remove", TimeSpan.FromMilliseconds(0));
			static public readonly IoDelayType Rename = new IoDelayType("Rename", TimeSpan.FromMilliseconds(0));
			static public readonly IoDelayType Read = new IoDelayType("Read", TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(4), 0x10000);
			static public readonly IoDelayType Write = new IoDelayType("Write", TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(5), 0x10000);

			static public readonly IoDelayType GetStat = new IoDelayType("GetStat", TimeSpan.FromMilliseconds(5));
			static public readonly IoDelayType ChStat = new IoDelayType("ChStat", TimeSpan.FromMilliseconds(5));

			static public readonly IoDelayType Dopen = new IoDelayType("Dopen", TimeSpan.FromMilliseconds(0));
			static public readonly IoDelayType Dread = new IoDelayType("Dread", TimeSpan.FromMilliseconds(0));
			static public readonly IoDelayType Dclose = new IoDelayType("Dclose", TimeSpan.FromMilliseconds(0));

			static public readonly IoDelayType Mkdir = new IoDelayType("Mkdir", TimeSpan.FromMilliseconds(0));
			static public readonly IoDelayType Chdir = new IoDelayType("Chdir", TimeSpan.FromMilliseconds(0));
			static public readonly IoDelayType Rmdir = new IoDelayType("Rmdir", TimeSpan.FromMilliseconds(0));

			string Name;
			TimeSpan BaseDelay;
			TimeSpan DelayPerUnit;
			int UnitSize = 1;

			private IoDelayType(string Name, TimeSpan BaseDelay, TimeSpan DelayPerUnit = default(TimeSpan), int UnitSize = 1)
			{
				this.Name = Name;
				this.BaseDelay = BaseDelay;
				this.DelayPerUnit = DelayPerUnit;
				this.UnitSize = UnitSize;
			}

			public TimeSpan GetTimePerSize(long Size)
			{
				return this.BaseDelay + TimeSpan.FromMilliseconds(((DelayPerUnit.TotalMilliseconds * Size) / UnitSize));
			}

			public override string ToString()
			{
				return String.Format("IoDelayType(Name={0}, BaseDelay={1}, DelayPerUnit={2}, UnitSize={3})", Name, BaseDelay, DelayPerUnit, UnitSize);
			}
		}

		private void _DelayIo(IoDelayType IoDelayType, long DataSize = 1)
		{
			//return;

			var TimeSpan = IoDelayType.GetTimePerSize(DataSize);

			//Console.WriteLine("_DelayIo: {0}, {1} : {2}", IoDelayType, DataSize, TimeSpan);

			if (TimeSpan != TimeSpan.Zero)
			{
				var CurrentThread = ThreadManager.Current;
				if (CurrentThread != null)
				{
					//ThreadManager
					CurrentThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Timer, "_DelayIo", null, WakeUpCallback =>
					{
						PspRtc.RegisterTimerInOnce(TimeSpan, () =>
						{
							WakeUpCallback();
						});
					}, HandleCallbacks: false);
				}
			}
			else
			{
				ThreadManager.Reschedule();
			}
		}
	}
}
