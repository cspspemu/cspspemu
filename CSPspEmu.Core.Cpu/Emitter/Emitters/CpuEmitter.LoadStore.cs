using System;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Byte/Half word/Word (Left/Right/Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lb() { return AssignGPR(RT, AstMemoryGetValue<sbyte>(Memory, Address_RS_IMM())); }
		public AstNodeStm lbu() { return AssignGPR(RT, AstMemoryGetValue<byte>(Memory, Address_RS_IMM())); }
		public AstNodeStm lh() { return AssignGPR(RT, AstMemoryGetValue<short>(Memory, Address_RS_IMM())); }
		public AstNodeStm lhu() { return AssignGPR(RT, AstMemoryGetValue<ushort>(Memory, Address_RS_IMM())); }
		public AstNodeStm lw() { return AssignGPR(RT, AstMemoryGetValue<int>(Memory, Address_RS_IMM())); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Store Byte/Half word/Word (Left/Right).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm sb() { return AstMemorySetValue<byte>(Memory, Address_RS_IMM(), GPR_u(RT)); }
		public AstNodeStm sh() { return AstMemorySetValue<ushort>(Memory, Address_RS_IMM(), GPR_u(RT)); }
		public AstNodeStm sw() { return AstMemorySetValue<uint>(Memory, Address_RS_IMM(), GPR_u(RT)); }

		public AstNodeStm lwl() { return AssignGPR(RT, ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwl_exec, CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }
		public AstNodeStm lwr() { return AssignGPR(RT, ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwr_exec, CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }

		public AstNodeStm swl() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swl_exec, CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }
		public AstNodeStm swr() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swr_exec, CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Linked word.
		// Store Conditional word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm ll() { throw (new NotImplementedException()); }
		public AstNodeStm sc() { throw (new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Word to Cop1 floating point.
		// Store Word from Cop1 floating point.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lwc1() { return AssignFPR_I(FT, AstMemoryGetValue<int>(Memory, this.Address_RS_IMM())); }
		public AstNodeStm swc1() { return AstMemorySetValue<int>(Memory, this.Address_RS_IMM(), FPR_I(FT)); }
	}
}