using System;
using SafeILGenerator.Ast.Nodes;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		//Logger Logger = Logger.GetLogger("CpuEmitter");

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Syscall
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm syscall() {
#if true
			if (Instruction.CODE == SyscallInfo.NativeCallSyscallCode)
			{
				var DelegateId = Memory.Read4(PC + 4);
				var SyscallInfoInfo = CpuProcessor.RegisteredNativeSyscallMethods[DelegateId];
				return ast.Statements(
					ast.Assign(REG("PC"), PC),
					ast.Comment(SyscallInfoInfo.Name),
					ast.Statement(ast.CallInstance(MipsMethodEmitter.CpuThreadStateArgument(), (Action)CpuThreadState.Methods.Tick)),
					ast.Statement(ast.CallDelegate(SyscallInfoInfo.PoolItem.GetAstFieldAccess(), CpuThreadStateArgument()))
				);
			}
			else
#endif
			{
				return ast.Statements(
					ast.Assign(REG("PC"), PC),
					ast.Statement(ast.CallInstance(MipsMethodEmitter.CpuThreadStateArgument(), (Action)CpuThreadState.Methods.Tick)),
					ast.Statement(ast.CallInstance(CpuThreadStateArgument(), (Action<int>)CpuThreadState.Methods.Syscall, (int)Instruction.CODE))
				);
			}
		}
		public AstNodeStm cache() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._cache_impl, CpuThreadStateArgument(), PC, (uint)Instruction.Value)); }
		public AstNodeStm sync() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._sync_impl, CpuThreadStateArgument(), PC, (uint)Instruction.Value)); }
		public AstNodeStm _break() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._break_impl, CpuThreadStateArgument(), PC, (uint)Instruction.Value)); }
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
			Console.Error.WriteLine("UNKNOWN INSTRUCTION: 0x{0:X8} : 0x{1:X8} at 0x{2:X8}", Instruction.Value, Instruction.Value, PC);
			//return _break();
			return ast.Statement();
		}
	}
}
