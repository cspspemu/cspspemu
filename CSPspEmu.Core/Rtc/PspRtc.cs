using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Rtc
{
	unsafe public class PspRtc : PspEmulatorComponent
	{
		static public Logger Logger = Logger.GetLogger("Rtc");

		public class VirtualTimer
		{
			protected PspRtc PspRtc;

			protected DateTime _DateTime;

			public DateTime DateTime
			{
				set
				{
					lock (PspRtc.Timers)
					{
						lock (this)
						{
							this._DateTime = value;
							if (!OnList)
							{
								PspRtc.Timers.AddLast(this);
								OnList = true;
							}
						}
					}
				}
				get
				{
					return _DateTime;
				}
			}
			public bool OnList;

			internal Action Callback;
			public bool Enabled;

			internal VirtualTimer(PspRtc PspRtc)
			{
				this.PspRtc = PspRtc;
			}

			public void SetIn(TimeSpan TimeSpan)
			{
				this.DateTime = DateTime.UtcNow + TimeSpan;
			}

			public void SetAt(DateTime DateTime)
			{
				this.DateTime = DateTime;
			}

			public override string ToString()
			{
				return this.ToStringDefault();
			}
		}

		public struct PspTimeStruct
		{
			public long TotalMicroseconds;

			public void SetToNow()
			{
				var PrevTotalMicroseconds = TotalMicroseconds;
				var CurrentTotalMicroseconds = Platform.GetCurrentMicroseconds();

				if (CurrentTotalMicroseconds < PrevTotalMicroseconds)
				{
					ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
					{
						Logger.Error("Total Microseconds overflow Prev({0}), Now({1})", PrevTotalMicroseconds, CurrentTotalMicroseconds);
					});
				}
				this.TotalMicroseconds = CurrentTotalMicroseconds;
			}

			public long TotalMilliseconds
			{
				get
				{
					return TotalMicroseconds / 1000;
				}
			}
		}

		protected LinkedList<VirtualTimer> Timers = new LinkedList<VirtualTimer>();
		public DateTime StartDateTime;
		public DateTime CurrentDateTime;
		//private DateTime CurrentDateTime;

		protected PspTimeStruct StartTime;
		protected PspTimeStruct CurrentTime;
		public PspTimeStruct ElapsedTime;

		public TimeSpan Elapsed
		{
			get {
				return CurrentDateTime - StartDateTime;
			}
		}

		public DateTime UpdatedCurrentDateTime
		{
			get
			{
				Update();
				return CurrentDateTime;
			}
		}

		public uint UnixTimeStamp
		{
			get
			{
				return (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			}
		}

		public override void InitializeComponent()
		{
			Start();
		}

		public void Start()
		{
			this.StartDateTime = DateTime.UtcNow;
			this.StartTime.SetToNow();
		}

		public void Update()
		{
			CurrentTime.SetToNow();
			this.CurrentDateTime = DateTime.UtcNow;
			ElapsedTime.TotalMicroseconds = CurrentTime.TotalMicroseconds - StartTime.TotalMicroseconds;

			lock (Timers)
			{
			RetryLoop:
				foreach (var Timer in Timers)
				{
					lock (Timer)
					{
						if (Timer.Enabled && this.CurrentDateTime >= Timer.DateTime)
						{
							Timers.Remove(Timer);
							Timer.Callback();
							Timer.OnList = false;
							goto RetryLoop;
						}
					}
				}
			}
		}

		public VirtualTimer CreateVirtualTimer(Action Callback)
		{
			return new VirtualTimer(this)
			{
				Callback = Callback,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TimeSpan"></param>
		/// <param name="Callback"></param>
		public VirtualTimer RegisterTimerInOnce(TimeSpan TimeSpan, Action Callback)
		{
			Logger.Notice("RegisterTimerInOnce: " + TimeSpan);
			return RegisterTimerAtOnce(UpdatedCurrentDateTime + TimeSpan, Callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DateTime"></param>
		/// <param name="Callback"></param>
		public VirtualTimer RegisterTimerAtOnce(DateTime DateTime, Action Callback)
		{
			lock (Timers)
			{
				Logger.Notice("RegisterTimerAtOnce: " + DateTime);
				var VirtualTimer = CreateVirtualTimer(Callback);
				VirtualTimer.SetAt(DateTime);
				VirtualTimer.Enabled = true;
				return VirtualTimer;
			}
		}

		public void RegisterTimeout(uint* Timeout, Action WakeUpCallback)
		{
			if (Timeout != null)
			{
				RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(*Timeout), () =>
				{
					WakeUpCallback();
				});
			}
		}
	}
}
