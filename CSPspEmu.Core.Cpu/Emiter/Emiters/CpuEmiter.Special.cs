using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		// Syscall
		public void syscall()
		{
#if false
			var Code = (int)Instruction.CODE;
			var Action = CpuProcessor.GetSyscall(Code);
			if (Action != null)
			{
				//MipsMethodEmiter.ILGenerator.Emit(OpCodes., Action.Target);
				SafeILGenerator.LoadArgument0CpuThreadState()
				SafeILGenerator.Push((int)Code);
				SafeILGenerator.Call(Action.Method);
			}
			else
			{
				Console.WriteLine("Undefined syscall: %06X at 0x%08X".Sprintf(Code, PC));
			}
#else
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((int)Instruction.CODE);
			SafeILGenerator.Call((Action<int>)CpuThreadState.Methods.Syscall);
#endif
		}

		static public void _cache_impl(CpuThreadState CpuThreadState, uint Value)
		{
			//Console.Error.WriteLine("cache! : 0x{0:X}", Value);
			//CpuThreadState.CpuProcessor.sceKernelIcacheInvalidateAll();
		}

		public void cache() {
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((int)(uint)Instruction.Value);
			SafeILGenerator.Call((Action<CpuThreadState, uint>)CpuEmiter._cache_impl);
		}
		public void sync() {
			Console.WriteLine("Not implemented 'sync' instruction");
		}

		static public void _break_impl(CpuThreadState CpuThreadState)
		{
			Console.Error.WriteLine("-------------------------------------------------------------------");
			Console.Error.WriteLine("-- BREAK  ---------------------------------------------------------");
			Console.Error.WriteLine("-------------------------------------------------------------------");
			throw(new PspBreakException("Break!"));
		}

		public void _break() {
			_save_pc();
			//CpuThreadState.PC =
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Call((Action<CpuThreadState>)CpuEmiter._break_impl);
		}
		public void dbreak() { throw(new NotImplementedException()); }
		public void halt() { throw(new NotImplementedException()); }

		// (D?/Exception) RETurn
		public void dret() { throw(new NotImplementedException()); }
		public void eret() { throw(new NotImplementedException()); }

		// Move (From/To) IC
		public void mfic()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadFieldPtr(typeof(CpuThreadState).GetField("IC"));
				SafeILGenerator.LoadIndirect<int>();
			});
		}
		public void mtic()
		{
			MipsMethodEmiter.SaveField<int>(typeof(CpuThreadState).GetField("IC"), () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
			});
		}

		// Move (From/To) DR
		public void mfdr() { throw(new NotImplementedException()); }
		public void mtdr() { throw(new NotImplementedException()); }

		public void unknown()
		{
			throw (new NotImplementedException("0x%08X : %032b at 0x%08X".Sprintf(Instruction.Value, Instruction.Value, PC)));
		}
	}
}
