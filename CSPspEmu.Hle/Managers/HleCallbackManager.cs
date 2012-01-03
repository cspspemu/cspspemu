using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Managers
{
	public class HleCallbackManager : PspEmulatorComponent
	{
		public HleUidPool<HleCallback> Callbacks { get; protected set; }
		private Queue<HleCallback> ScheduledCallbacks;
		private CpuProcessor CpuProcessor;
		private HleInterop HleInterop;

		public override void InitializeComponent()
		{
			this.Callbacks = new HleUidPool<HleCallback>();
			this.ScheduledCallbacks = new Queue<HleCallback>();
			this.CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			this.HleInterop = PspEmulatorContext.GetInstance<HleInterop>();
		}

		public void ScheduleCallback(HleCallback HleCallback)
		{
			Console.WriteLine("ScheduleCallback!");
			lock (this)
			{
				this.ScheduledCallbacks.Enqueue(HleCallback);
			}
		}

		public HleCallback DequeueScheduledCallback()
		{
			Console.WriteLine("DequeueScheduledCallback!");
			return ScheduledCallbacks.Dequeue();
		}

		public bool HasScheduledCallbacks
		{
			get
			{
				lock (this)
				{
					return ScheduledCallbacks.Count > 0;
				}
			}
		}

		public int ExecuteQueued(CpuThreadState CpuThreadState, bool MustReschedule)
		{
			int ExecutedCount = 0;

			//Console.WriteLine("ExecuteQueued");
			if (HasScheduledCallbacks)
			{
				//Console.WriteLine("ExecuteQueued.HasScheduledCallbacks!");
				//Console.Error.WriteLine("STARTED CALLBACKS");
				while (HasScheduledCallbacks)
				{
					var HleCallback = DequeueScheduledCallback();

					var FakeCpuThreadState = new CpuThreadState(CpuProcessor);
					FakeCpuThreadState.CopyRegistersFrom(CpuThreadState);
					HleCallback.SetArgumentsToCpuThreadState(FakeCpuThreadState);

					try
					{
						HleInterop.Execute(FakeCpuThreadState);
					}
					finally
					{
						ExecutedCount++;
					}

					//Console.Error.WriteLine("  CALLBACK ENDED : " + HleCallback);
					if (MustReschedule)
					{
						//Console.Error.WriteLine("    RESCHEDULE");
						break;
					}
				}
				//Console.Error.WriteLine("ENDED CALLBACKS");
			}

			return ExecutedCount;
		}
	}
}
