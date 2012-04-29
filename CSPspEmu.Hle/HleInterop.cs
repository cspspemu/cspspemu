using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle
{
	public class HleInterop : PspEmulatorComponent
	{
		protected HleThread CurrentFakeHleThread;

		[Inject]
		protected HleThreadManager HleThreadManager;

		[Inject]
		protected CpuProcessor CpuProcessor;

		public override void InitializeComponent()
		{
			//throw new NotImplementedException();
		}

		public uint ExecuteFunctionNow(uint Function, params object[] Arguments)
		{
			CurrentFakeHleThread = new HleThread(HleThreadManager, new CpuThreadState(CpuProcessor));
			CurrentFakeHleThread.CpuThreadState.CopyRegistersFrom(HleThreadManager.CurrentOrAny.CpuThreadState);
			SetArgumentsToCpuThreadState(CurrentFakeHleThread.CpuThreadState, Function, Arguments);
			{
				CurrentFakeHleThread.CpuThreadState.RA = HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK;

				CpuProcessor.RunningCallback = true;
				while (CpuProcessor.RunningCallback)
				{
					CurrentFakeHleThread.Step();
				}
			}
			return (uint)CurrentFakeHleThread.CpuThreadState.GPR2;
		}

		public class QueuedExecution
		{
			public uint Function;
			public Action<uint> ExecutedCallback;
			public object[] Arguments;
		}

		Queue<QueuedExecution> QueuedExecutions = new Queue<QueuedExecution>();

		public int ExecuteAllQueuedFunctionsNow()
		{
			lock (QueuedExecutions)
			{
				int ExecutedCount = 0;
				while (QueuedExecutions.Count > 0)
				{
					var QueuedExecution = QueuedExecutions.Dequeue();
					var Result = ExecuteFunctionNow(QueuedExecution.Function, QueuedExecution.Arguments);
					if (QueuedExecution.ExecutedCallback != null) QueuedExecution.ExecutedCallback(Result);
					ExecutedCount++;
				}
				return ExecutedCount;
			}
		}

		public void ExecuteFunctionLater(uint Function, Action<uint> ExecutedCallback, params object[] Arguments)
		{
			lock (QueuedExecutions)
			{
				QueuedExecutions.Enqueue(new QueuedExecution()
				{
					Function = Function,
					ExecutedCallback = ExecutedCallback,
					Arguments = Arguments,
				});
			}
		}

		public HleThread Execute(CpuThreadState FakeCpuThreadState)
		{
			var CpuProcessor = FakeCpuThreadState.CpuProcessor;
			if (CurrentFakeHleThread == null)
			{
				CurrentFakeHleThread = new HleThread(HleThreadManager, new CpuThreadState(CpuProcessor));
			}

			CurrentFakeHleThread.CpuThreadState.CopyRegistersFrom(FakeCpuThreadState);
			//HleCallback.SetArgumentsToCpuThreadState(CurrentFake.CpuThreadState);

			//CurrentFake.CpuThreadState.PC = HleCallback.Function;
			CurrentFakeHleThread.CpuThreadState.RA = HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK;
			//Current.CpuThreadState.RA = 0;

			CpuProcessor.RunningCallback = true;
			while (CpuProcessor.RunningCallback)
			{
				//Console.WriteLine("AAAAAAA {0:X}", CurrentFake.CpuThreadState.PC);
				CurrentFakeHleThread.Step();
			}

			return CurrentFakeHleThread;
		}

		static public void SetArgumentsToCpuThreadState(CpuThreadState CpuThreadState, uint Function, params object[] Arguments)
		{
			int GprIndex = 4;
			//int FprIndex = 0;
			Action<int> GprAlign = (int Alignment) =>
			{
				GprIndex = (int)MathUtils.NextAligned((uint)GprIndex, Alignment);
			};
			foreach (var Argument in Arguments)
			{
				var ArgumentType = Argument.GetType();
				if (ArgumentType == typeof(uint))
				{
					GprAlign(1);
					CpuThreadState.GPR[GprIndex++] = (int)(uint)Argument;
				}
				else if (ArgumentType == typeof(int))
				{
					GprAlign(1);
					CpuThreadState.GPR[GprIndex++] = (int)Argument;
				}
				else if (ArgumentType == typeof(PspPointer))
				{
					GprAlign(1);
					CpuThreadState.GPR[GprIndex++] = (int)(uint)(PspPointer)Argument;
				}
				else
				{
					throw (new NotImplementedException(String.Format("Can't handle type '{0}'", ArgumentType)));
				}
			}

			CpuThreadState.PC = Function;
			//Console.Error.WriteLine(CpuThreadState);
			//CpuThreadState.DumpRegisters(Console.Error);
		}
	}
}
