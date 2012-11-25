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
		public void lb() { GenerateAssignGPR(RT, this.AstMemoryGetValue<sbyte>(Address_RS_IMM())); }
		public void lbu() { GenerateAssignGPR(RT, this.AstMemoryGetValue<byte>(Address_RS_IMM())); }
		public void lh() { GenerateAssignGPR(RT, this.AstMemoryGetValue<short>(Address_RS_IMM())); }
		public void lhu() { GenerateAssignGPR(RT, this.AstMemoryGetValue<ushort>(Address_RS_IMM())); }
		public void lw() { GenerateAssignGPR(RT, this.AstMemoryGetValue<int>(Address_RS_IMM())); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Store Byte/Half word/Word (Left/Right).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void sb() { this.GenerateIL(this.AstMemorySetValue<byte>(Address_RS_IMM(), GPR_u(RT))); }
		public void sh() { this.GenerateIL(this.AstMemorySetValue<ushort>(Address_RS_IMM(), GPR_u(RT))); }
		public void sw() { this.GenerateIL(this.AstMemorySetValue<uint>(Address_RS_IMM(), GPR_u(RT))); }

		public void lwl() { this.GenerateAssignGPR(RT, this.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwl_exec, this.CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }
		public void lwr() { this.GenerateAssignGPR(RT, this.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwr_exec, this.CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT))); }

		public void swl() { this.GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swl_exec, this.CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT)))); }
		public void swr() { this.GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swr_exec, this.CpuThreadStateArgument(), GPR_u(RS), IMM_s(), GPR_u(RT)))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Linked word.
		// Store Conditional word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void ll() { throw (new NotImplementedException()); }
		public void sc() { throw (new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Word to Cop1 floating point.
		// Store Word from Cop1 floating point.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void lwc1() { GenerateAssignFPR_I(FT, AstMemoryGetValue<int>(this.Address_RS_IMM())); }
		public void swc1() { this.GenerateIL(AstMemorySetValue<int>(this.Address_RS_IMM(), FPR_I(FT))); }
	}
}