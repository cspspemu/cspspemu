using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Modules.emulator
{
	unsafe public partial class Emulator : HleModuleHost
	{
		[HlePspFunction(NID = 0x00000000, FirmwareVersion = 150)]
		public void emitInt(int Value)
		{
			Console.WriteLine("emitInt: {0}", Value);
		}

		[HlePspFunction(NID = 0x00000001, FirmwareVersion = 150)]
		public void emitFloat(float Value)
		{
			Console.WriteLine("emitFloat: {0:0.00000}", Value);
		}

		[HlePspFunction(NID = 0x00000002, FirmwareVersion = 150)]
		public void emitString(string Value)
		{
			Console.WriteLine("emitString: '{0}'", Value);
		}

		[HlePspFunction(NID = 0x00000003, FirmwareVersion = 150)]
		public void emitMemoryBlock(byte* Value, uint Size)
		{
			throw(new NotImplementedException());
		}

		[HlePspFunction(NID = 0x00000004, FirmwareVersion = 150)]
		public void emitHex(byte* Value, uint Size)
		{
			throw(new NotImplementedException());
		}

		[HlePspFunction(NID = 0x00000005, FirmwareVersion = 150)]
		public void emitUInt(uint Value)
		{
			Console.WriteLine("emitUInt: {0}", "0x%08X".Sprintf(Value));
		}

		[HlePspFunction(NID = 0x10000000, FirmwareVersion = 150)]
		public void waitThreadForever(CpuThreadState CpuThreadState)
		{
			var SleepThread = HleState.ThreadManager.Current;
			SleepThread.CurrentStatus = HleThread.Status.Waiting;
			SleepThread.CurrentWaitType = HleThread.WaitType.None;
			CpuThreadState.Yield();
		}
	}
}
