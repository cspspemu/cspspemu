using System;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Byte/Half word/Word (Left/Right/Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lb() { return ast.AssignGPR(RT, ast.AstMemoryGetValue<sbyte>(Memory, Address_RS_IMM())); }
		public AstNodeStm lbu() { return ast.AssignGPR(RT, ast.AstMemoryGetValue<byte>(Memory, Address_RS_IMM())); }
		public AstNodeStm lh() { return ast.AssignGPR(RT, ast.AstMemoryGetValue<short>(Memory, Address_RS_IMM())); }
		public AstNodeStm lhu() { return ast.AssignGPR(RT, ast.AstMemoryGetValue<ushort>(Memory, Address_RS_IMM())); }
		public AstNodeStm lw() { return ast.AssignGPR(RT, ast.AstMemoryGetValue<int>(Memory, Address_RS_IMM())); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Store Byte/Half word/Word (Left/Right).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm sb() { return ast.AstMemorySetValue<byte>(Memory, Address_RS_IMM(), ast.GPR_u(RT)); }
		public AstNodeStm sh() { return ast.AstMemorySetValue<ushort>(Memory, Address_RS_IMM(), ast.GPR_u(RT)); }
		public AstNodeStm sw() { return ast.AstMemorySetValue<uint>(Memory, Address_RS_IMM(), ast.GPR_u(RT)); }

		public AstNodeStm lwl() { return ast.AssignGPR(RT, ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwl_exec, ast.CpuThreadStateArgument(), ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT))); }
		public AstNodeStm lwr() { return ast.AssignGPR(RT, ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwr_exec, ast.CpuThreadStateArgument(), ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT))); }

		public AstNodeStm swl() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swl_exec, ast.CpuThreadStateArgument(), ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT))); }
		public AstNodeStm swr() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swr_exec, ast.CpuThreadStateArgument(), ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Linked word.
		// Store Conditional word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm ll() { throw (new NotImplementedException("ll")); }
		public AstNodeStm sc() { throw (new NotImplementedException("sc")); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Word to Cop1 floating point.
		// Store Word from Cop1 floating point.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lwc1() { return ast.AssignFPR_I(FT, ast.AstMemoryGetValue<int>(Memory, this.Address_RS_IMM())); }
		public AstNodeStm swc1() { return ast.AstMemorySetValue<int>(Memory, this.Address_RS_IMM(), ast.FPR_I(FT)); }
	}
}