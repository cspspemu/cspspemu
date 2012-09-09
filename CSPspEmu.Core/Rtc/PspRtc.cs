using System;
using System.Collections.Generic;
using CSharpUtils;

namespace CSPspEmu.Core.Rtc
{
	unsafe public class PspRtc : PspEmulatorComponent
	{
		static public Logger Logger = Logger.GetLogger("Rtc");

		internal LinkedList<PspVirtualTimer> Timers = new LinkedList<PspVirtualTimer>();
		public DateTime StartDateTime { get; protected set; }
		public DateTime CurrentDateTime { get; protected set; }

		protected PspTimeStruct StartTime;
		protected PspTimeStruct CurrentTime;
		public PspTimeStruct ElapsedTime { get { return _ElapsedTime; } }
		protected PspTimeStruct _ElapsedTime;

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
		public override void InitializeComponent()
		{
			Start();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Start()
		{
			this.StartDateTime = DateTime.UtcNow;
			this.StartTime.SetToNow();
		}

		protected virtual void UpdateInternal()
		{
			CurrentTime.SetToNow();
			this.CurrentDateTime = DateTime.UtcNow;
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Callback"></param>
		/// <returns></returns>
		public PspVirtualTimer CreateVirtualTimer(Action Callback)
		{
			return new PspVirtualTimer(this)
			{
				Callback = Callback,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TimeSpan"></param>
		/// <param name="Callback"></param>
		public PspVirtualTimer RegisterTimerInOnce(TimeSpan TimeSpan, Action Callback)
		{
			Logger.Notice("RegisterTimerInOnce: " + TimeSpan);
			Update();
			return RegisterTimerAtOnce(CurrentDateTime + TimeSpan, Callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DateTime"></param>
		/// <param name="Callback"></param>
		public PspVirtualTimer RegisterTimerAtOnce(DateTime DateTime, Action Callback)
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Timeout"></param>
		/// <param name="WakeUpCallback"></param>
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
