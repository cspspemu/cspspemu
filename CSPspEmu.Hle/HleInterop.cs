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
		protected HleThreadManager HleThreadManager;
		protected CpuProcessor CpuProcessor;

		public override void InitializeComponent()
		{
			this.CpuProcessor = this.PspEmulatorContext.GetInstance<CpuProcessor>();
			this.HleThreadManager = this.PspEmulatorContext.GetInstance<HleThreadManager>();
			//throw new NotImplementedException();
		}

		public uint ExecuteFunctionNow(uint Function, params object[] Arguments)
		{
			CurrentFakeHleThread = new HleThread(new CpuThreadState(CpuProcessor));
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

		public void ExecuteFunctionLater(uint Function, Action<uint> ExecutedCallback, params object[] Arguments)
		{
			/*
			var Result = ExecuteFunctionNow(Function, Arguments);
			ExecutedCallback(Result);
			*/
			throw(new NotImplementedException());
		}

		public HleThread Execute(CpuThreadState FakeCpuThreadState)
		{
			var CpuProcessor = FakeCpuThreadState.CpuProcessor;
			if (CurrentFakeHleThread == null)
			{
				CurrentFakeHleThread = new HleThread(new CpuThreadState(CpuProcessor));
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
