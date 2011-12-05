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

		public override void InitializeComponent()
		{
			this.Callbacks = new HleUidPool<HleCallback>();
			this.ScheduledCallbacks = new Queue<HleCallback>();
			this.CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
		}

		public void ScheduleCallback(HleCallback HleCallback)
		{
			lock (this)
			{
				this.ScheduledCallbacks.Enqueue(HleCallback);
			}
		}

		public HleCallback DequeueScheduledCallback()
		{
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

			if (HasScheduledCallbacks)
			{
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
