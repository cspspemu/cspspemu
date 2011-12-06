using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle
{
	public class HleInterop
	{
		static public void Execute(CpuThreadState FakeCpuThreadState)
		{
			var CpuProcessor = FakeCpuThreadState.CpuProcessor;
			using (var CurrentFake = new HleThread(new CpuThreadState(CpuProcessor)))
			{
				CurrentFake.CpuThreadState.CopyRegistersFrom(FakeCpuThreadState);
				//HleCallback.SetArgumentsToCpuThreadState(CurrentFake.CpuThreadState);

				//CurrentFake.CpuThreadState.PC = HleCallback.Function;
				CurrentFake.CpuThreadState.RA = HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK;
				//Current.CpuThreadState.RA = 0;

				CpuProcessor.RunningCallback = true;
				while (CpuProcessor.RunningCallback)
				{
					//Console.WriteLine("AAAAAAA {0:X}", CurrentFake.CpuThreadState.PC);
					CurrentFake.Step();
				}
			}
		}
	}
}
