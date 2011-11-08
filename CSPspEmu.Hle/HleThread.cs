using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Threading;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle
{
	public class HleThread
	{
		public int Priority = 1;
		public int PriorityValue;
		protected GreenThread GreenThread;
		public CpuThreadState CpuThreadState { get; protected set; }
		protected MethodCache MethodCache;
		protected int MinimalInstructionCountForYield = 1000000;
		public int Id;
		public Status CurrentStatus;
		public WaitType CurrentWaitType;
		public DateTime AwakeOnTime;

		public enum WaitType
		{
			None = 0,
			Timer = 1,
		}

		public enum Status {
			Running = 1,
			Ready = 2,
			Waiting = 4,
			Suspend = 8,
			Stopped = 16,
			Killed = 32,
		}

		public HleThread(CpuThreadState CpuThreadState)
		{
			this.GreenThread = new GreenThread();
			this.CpuThreadState = CpuThreadState;
			this.MethodCache = CpuThreadState.Processor.MethodCache;
			this.PrepareThread();
		}

		protected void PrepareThread()
		{
			GreenThread.InitAndStartStopped(MainLoop);
		}

		protected void MainLoop()
		{
			while (true)
			{
				//Console.WriteLine("aa");
				GetDelegateAt(CpuThreadState.PC)(CpuThreadState);
			}
			/*
			long InstructionCountAfterLastYield = 0;
			while (true)
			{
				GetDelegateAt(CpuThreadState.PC)(CpuThreadState);

				int CurrentStepInstructionCount = CpuThreadState.StepInstructionCount;
				CpuThreadState.TotalInstructionCount += CurrentStepInstructionCount;
				InstructionCountAfterLastYield += CurrentStepInstructionCount;

				CpuThreadState.StepInstructionCount = 0;
				//Console.WriteLine(CurrentStepInstructionCount);

				if (InstructionCountAfterLastYield >= MinimalInstructionCountForYield)
				{
					GreenThread.Yield();
					InstructionCountAfterLastYield = 0;
				}

				//Console.WriteLine(CpuThreadState.StepInstructionCount);
				//GreenThread.Yield();
			}
			*/
		}

		public Action<CpuThreadState> GetDelegateAt(uint PC)
		{
			var Delegate = MethodCache.TryGetMethodAt(PC);
			if (Delegate == null)
			{
				MethodCache.SetMethodAt(
					PC,
					Delegate = CpuThreadState.CreateDelegateForPC(new PspMemoryStream(CpuThreadState.Processor.Memory), PC)
				);
			}

			return Delegate;
		}

		public void Step(int InstructionCountForYield = 1000000)
		{
			CpuThreadState.StepInstructionCount = InstructionCountForYield;
			//this.MinimalInstructionCountForYield = InstructionCountForYield;
			GreenThread.SwitchTo();
		}
	}
}
