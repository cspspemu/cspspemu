using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
	class Program
	{
		static public string Flash0Directory
		{
			get
			{
				return @"..\..\..\deploy\cspspemu\flash0";
			}
		}

		static void Main(string[] args)
		{
			var PspConfig = new PspConfig();
			var PspEmulatorContext = new PspEmulatorContext(PspConfig);
			PspEmulatorContext.SetInstanceType<PspMemory, NormalPspMemory>();
			var CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			var CpuThreadState = new CpuThreadState(CpuProcessor);
			CpuProcessor.Memory.WriteBytes(0x08600000, File.ReadAllBytes(Flash0Directory + @"\reboot.bin"));
			var CachedGetMethodCache = PspEmulatorContext.GetInstance<CachedGetMethodCache>();

			CpuThreadState.PC = 0x08600000;
			while (true)
			{
				var PC = CpuThreadState.PC & 0x0FFFFFFF;
				var Func = CachedGetMethodCache.GetDelegateAt(PC);
				Console.WriteLine("PC:{0:X8}", PC);
				Func.Delegate(CpuThreadState);
			}
		}
	}
}
