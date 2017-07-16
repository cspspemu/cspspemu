using System;
using System.Collections.Generic;
using CSharpUtils;

namespace CSPspEmu.Core.Rtc
{
	public unsafe class PspRtc
	{
		public static Logger Logger = Logger.GetLogger("Rtc");

		internal LinkedList<PspVirtualTimer> Timers = new LinkedList<PspVirtualTimer>();
		public DateTime StartDateTime { get; protected set; }
		public DateTime CurrentDateTime { get; protected set; }

		protected PspTimeStruct StartTime;
		protected PspTimeStruct CurrentTime;
		public PspTimeStruct ElapsedTime => _ElapsedTime;
		private PspTimeStruct _ElapsedTime;

		/// <summary>
		/// 
		/// </summary>
		public TimeSpan Elapsed
		{
			get {
				return CurrentDateTime - StartDateTime;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public uint UnixTimeStamp
		{
			get
			{
				return (uint)(CurrentDateTime - new DateTime(1970, 1, 1)).TotalSeconds;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public PspRtc()
		{
			Start();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Start()
		{
			StartDateTime = DateTime.UtcNow;
			StartTime.SetToNow();
		}

		protected virtual void UpdateInternal()
		{
			CurrentTime.SetToNow();
			CurrentDateTime = DateTime.UtcNow;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Update()
		{
			UpdateInternal();
			_ElapsedTime.TotalMicroseconds = CurrentTime.TotalMicroseconds - StartTime.TotalMicroseconds;

			lock (Timers)
			{
			RetryLoop:
				foreach (var timer in Timers)
				{
					lock (timer)
					{
						if (timer.Enabled && CurrentDateTime >= timer.DateTime)
						{
							Timers.Remove(timer);
							timer.Callback();
							timer.OnList = false;
							goto RetryLoop;
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		/// <returns></returns>
		public PspVirtualTimer CreateVirtualTimer(Action callback)
		{
			return new PspVirtualTimer(this)
			{
				Callback = callback,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeSpan"></param>
		/// <param name="callback"></param>
		public PspVirtualTimer RegisterTimerInOnce(TimeSpan timeSpan, Action callback)
		{
			Logger.Notice("RegisterTimerInOnce: " + timeSpan);
			Update();
			return RegisterTimerAtOnce(CurrentDateTime + timeSpan, callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="callback"></param>
		public PspVirtualTimer RegisterTimerAtOnce(DateTime dateTime, Action callback)
		{
			lock (Timers)
			{
				Logger.Notice("RegisterTimerAtOnce: " + dateTime);
				var virtualTimer = CreateVirtualTimer(callback);
				virtualTimer.SetAt(dateTime);
				virtualTimer.Enabled = true;
				return virtualTimer;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeout"></param>
		/// <param name="wakeUpCallback"></param>
		public void RegisterTimeout(uint* timeout, Action wakeUpCallback)
		{
			if (timeout != null)
			{
				RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(*timeout), () =>
				{
					wakeUpCallback();
				});
			}
		}
	}
}
