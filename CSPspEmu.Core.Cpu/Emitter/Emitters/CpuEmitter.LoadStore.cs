using System;
using CSPspEmu.Core.Memory;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Byte/Half word/Word (Left/Right/Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lb() { return AssignGPR(RT, this.AstMemoryGetValue<sbyte>(Address_RS_IMM())); }
		public AstNodeStm lbu() { return AssignGPR(RT, this.AstMemoryGetValue<byte>(Address_RS_IMM())); }
		public AstNodeStm lh() { return AssignGPR(RT, this.AstMemoryGetValue<short>(Address_RS_IMM())); }
		public AstNodeStm lhu() { return AssignGPR(RT, this.AstMemoryGetValue<ushort>(Address_RS_IMM())); }
		public AstNodeStm lw() { return AssignGPR(RT, this.AstMemoryGetValue<int>(Address_RS_IMM())); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Store Byte/Half word/Word (Left/Right).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm sb() { return this.AstMemorySetValue<byte>(Address_RS_IMM(), GPR_u(RT)); }
		public AstNodeStm sh() { return this.AstMemorySetValue<ushort>(Address_RS_IMM(), GPR_u(RT)); }
		public AstNodeStm sw() { return this.AstMemorySetValue<uint>(Address_RS_IMM(), GPR_u(RT)); }

		public AstNodeStm lwl() { return AssignGPR(RT, ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwl_exec, this.CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }
		public AstNodeStm lwr() { return AssignGPR(RT, ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwr_exec, this.CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }

		public AstNodeStm swl() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swl_exec, this.CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }
		public AstNodeStm swr() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swr_exec, this.CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }

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
		public AstNodeStm lwc1() { return AssignFPR_I(FT, AstMemoryGetValue<int>(this.Address_RS_IMM())); }
		public AstNodeStm swc1() { return AstMemorySetValue<int>(this.Address_RS_IMM(), FPR_I(FT)); }
	}
}