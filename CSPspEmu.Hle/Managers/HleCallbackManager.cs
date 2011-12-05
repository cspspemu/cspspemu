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
					var CurrentFake = new HleThread(new CpuThreadState(CpuProcessor));
					CurrentFake.CpuThreadState.CopyRegistersFrom(CpuThreadState);
					try
					{
						//Console.Error.WriteLine("  CALLBACK STARTED : {0} AT {1}", HleCallback, CurrentFake);

						HleCallback.SetArgumentsToCpuThreadState(CurrentFake.CpuThreadState);

						CurrentFake.CpuThreadState.PC = HleCallback.Function;
						CurrentFake.CpuThreadState.RA = HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK;
						//Current.CpuThreadState.RA = 0;

						CpuProcessor.RunningCallback = true;
						while (CpuProcessor.RunningCallback)
						{
							//Console.WriteLine("AAAAAAA {0:X}", CurrentFake.CpuThreadState.PC);
							CurrentFake.Step();
						}
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
