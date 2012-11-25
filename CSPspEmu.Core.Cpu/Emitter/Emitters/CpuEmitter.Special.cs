using System;
using CSharpUtils;
using SafeILGenerator.Ast;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		Logger Logger = Logger.GetLogger("CpuEmitter");

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Syscall
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void syscall() { this.GenerateIL(this.Statement(this.CallInstance(this.CpuThreadStateArgument(), (Action<int>)CpuThreadState.Methods.Syscall, (int)Instruction.CODE))); }
		public void cache() { this.GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._cache_impl, this.CpuThreadStateArgument(), PC, (uint)Instruction.Value))); }
		public void sync() { this.GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._sync_impl, this.CpuThreadStateArgument(), PC, (uint)Instruction.Value))); }
		public void _break() { this.GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._break_impl, this.CpuThreadStateArgument(), PC, (uint)Instruction.Value))); }
		public void dbreak() { throw(new NotImplementedException()); }
		public void halt() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// (D?/Exception) RETurn
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void dret() { throw(new NotImplementedException()); }
		public void eret() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) IC
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void mfic() { this.GenerateAssignGPR(RT, REG("IC")); }
		public void mtic() { this.GenerateAssignREG(REG("IC"), GPR_u(RT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) DR
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void mfdr() { throw(new NotImplementedException()); }
		public void mtdr() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Unknown instruction
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void unknown()
		{
			Logger.Error("0x%08X : %032b at 0x%08X".Sprintf(Instruction.Value, Instruction.Value, PC));

			_break();
		}
	}
}
