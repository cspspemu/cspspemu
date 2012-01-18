using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		// Syscall
		public void syscall()
		{
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Instruction.CODE);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, MipsMethodEmiter.Method_Syscall);
		}

		static public void cache_impl(CpuThreadState CpuThreadState, uint Value)
		{
			//Console.Error.WriteLine("cache! : 0x{0:X}", Value);
			//CpuThreadState.CpuProcessor.sceKernelIcacheInvalidateAll();
		}

		public void cache() {
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, (uint)Instruction.Value);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("cache_impl"));
			//throw(new NotImplementedException());
		}
		public void sync() { throw(new NotImplementedException()); }

		static public void break_impl(CpuThreadState CpuThreadState)
		{
			Console.Error.WriteLine("-------------------------------------------------------------------");
			Console.Error.WriteLine("-- BREAK  ---------------------------------------------------------");
			Console.Error.WriteLine("-------------------------------------------------------------------");
			//throw(new Exception("Break!"));
		}

		public void _break() {
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("break_impl"));
			//throw(new NotImplementedException());
		}
		public void dbreak() { throw(new NotImplementedException()); }
		public void halt() { throw(new NotImplementedException()); }

		// (D?/Exception) RETurn
		public void dret() { throw(new NotImplementedException()); }
		public void eret() { throw(new NotImplementedException()); }

		// Move (From/To) IC
		public void mfic()
		{
			//throw (new NotImplementedException());
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadFieldPtr(typeof(CpuThreadState).GetField("IC"));
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I4);
			});
		}
		public void mtic()
		{
			//throw (new NotImplementedException());
			MipsMethodEmiter.SaveFieldI4(typeof(CpuThreadState).GetField("IC"), () =>
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
