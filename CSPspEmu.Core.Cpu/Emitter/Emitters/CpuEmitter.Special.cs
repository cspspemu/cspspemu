using System;
using CSharpUtils;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		Logger Logger = Logger.GetLogger("CpuEmitter");

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Syscall
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm syscall() { return ast.Statement(ast.CallInstance(this.CpuThreadStateArgument(), (Action<int>)CpuThreadState.Methods.Syscall, (int)Instruction.CODE)); }
		public AstNodeStm cache() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._cache_impl, this.CpuThreadStateArgument(), PC, (uint)Instruction.Value)); }
		public AstNodeStm sync() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._sync_impl, this.CpuThreadStateArgument(), PC, (uint)Instruction.Value)); }
		public AstNodeStm _break() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._break_impl, this.CpuThreadStateArgument(), PC, (uint)Instruction.Value)); }
		public AstNodeStm dbreak() { throw (new NotImplementedException("dbreak")); }
		public AstNodeStm halt() { throw (new NotImplementedException("halt")); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// (D?/Exception) RETurn
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm dret() { throw (new NotImplementedException("dret")); }
		public AstNodeStm eret() { throw (new NotImplementedException("eret")); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) IC
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfic() { return AssignGPR(RT, REG("IC")); }
		public AstNodeStm mtic() { return AssignREG("IC", GPR_u(RT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) DR
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfdr() { throw (new NotImplementedException("mfdr")); }
		public AstNodeStm mtdr() { throw (new NotImplementedException("mtdr")); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Unknown instruction
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm unknown()
		{
			Logger.Error("0x%08X : %032b at 0x%08X".Sprintf(Instruction.Value, Instruction.Value, PC));

			return _break();
		}
	}
}
