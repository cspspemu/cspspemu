using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe partial class MipsMethodEmitter
	{
		// AST UTILS

		public AstNodeExprArgument CpuThreadStateArgument() { return this.Argument<CpuThreadState>(0, "CpuThreadState"); }
		public AstNodeExprLValue FCR31_CC() { return this.FieldAccess(REG("Fcr31"), "CC"); }
		public AstNodeExprLValue REG(string RegName) { return this.FieldAccess(this.CpuThreadStateArgument(), RegName); }
		public AstNodeExprLValue GPR(int Index) { if (Index == 0) throw (new Exception("Can't get reference to GPR0")); return REG("GPR" + Index); }
		public AstNodeExprLValue GPR_l(int Index) { return this.Indirect(this.Cast(typeof(long*), this.GetAddress(GPR(Index)))); }
		public AstNodeExprLValue FPR(int Index) { return REG("FPR" + Index); }
		public AstNodeExprLValue FPR_I(int Index) { return this.Indirect(this.Cast(typeof(int*), this.GetAddress(REG("FPR" + Index)), Explicit: false)); }
		public AstNodeExpr GPR_s(int Index) { if (Index == 0) return this.Immediate((int)0); return this.Cast<int>(GPR(Index)); }
		public AstNodeExpr GPR_sl(int Index) { return this.Cast<long>(GPR_s(Index)); }
		public AstNodeExpr GPR_u(int Index) { if (Index == 0) return this.Immediate((uint)0); return this.Cast<uint>(GPR(Index)); }
		public AstNodeExpr GPR_ul(int Index) { return this.Cast<ulong>(GPR_u(Index)); }
		//public AstNodeExpr IMM_s() { return this.Immediate(IMM); }
		//public AstNodeExpr IMM_u() { return this.Immediate((uint)(ushort)IMM); }
		//public AstNodeExpr IMM_uex() { return this.Immediate((uint)IMM); }

		public AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return this.Assign(REG(RegName), Expr); }
		public AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return this.Assign(GPR(Index), this.Cast<uint>(Expr)); }


		delegate void* AddressToPointerFunc(uint Address);

		public AstNodeExpr AstMemoryGetPointer(AstNodeExpr Address, bool Safe, string ErrorDescription = "ERROR")
		{
			if (Safe)
			{
				return this.CallInstance(
					CpuThreadStateArgument(),
					(AddressToPointerFunc)CpuThreadState.Methods.GetMemoryPtrSafe,
					this.Cast<uint>(Address)
				);
			}
			else
			{
				return this.CallInstance(
					CpuThreadStateArgument(),
					(AddressToPointerFunc)CpuThreadState.Methods.GetMemoryPtr,
					this.Cast<uint>(Address)
				);
			}
		}
	}
}
