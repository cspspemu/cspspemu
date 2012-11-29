using System;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe partial class MipsMethodEmitter
	{
		// AST UTILS
		private static AstGenerator ast = AstGenerator.Instance;

		public static AstNodeExprArgument CpuThreadStateArgument() { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); }
		public static AstNodeExprLValue FCR31_CC() { return ast.FieldAccess(REG("Fcr31"), "CC"); }
		public static AstNodeExprLValue REG(string RegName) { return ast.FieldAccess(CpuThreadStateArgument(), RegName); }
		public static AstNodeExprLValue GPR(int Index) { if (Index == 0) throw (new Exception("Can't get reference to GPR0")); return REG("GPR" + Index); }
		public static AstNodeExprLValue GPR_l(int Index) { return ast.Indirect(ast.Cast(typeof(long*), ast.GetAddress(GPR(Index)))); }
		public static AstNodeExprLValue FPR(int Index) { return REG("FPR" + Index); }
		public static AstNodeExprLValue FPR_I(int Index) { return ast.Indirect(ast.Cast(typeof(int*), ast.GetAddress(REG("FPR" + Index)), Explicit: false)); }
		public static AstNodeExpr GPR_s(int Index) { if (Index == 0) return ast.Immediate((int)0); return ast.Cast<int>(GPR(Index)); }
		public static AstNodeExpr GPR_sl(int Index) { return ast.Cast<long>(GPR_s(Index)); }
		public static AstNodeExpr GPR_u(int Index) { if (Index == 0) return ast.Immediate((uint)0); return ast.Cast<uint>(GPR(Index)); }
		public static AstNodeExpr GPR_ul(int Index) { return ast.Cast<ulong>(GPR_u(Index)); }
		//public AstNodeExpr IMM_s() { return this.Immediate(IMM); }
		//public AstNodeExpr IMM_u() { return this.Immediate((uint)(ushort)IMM); }
		//public AstNodeExpr IMM_uex() { return this.Immediate((uint)IMM); }

		public AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(REG(RegName), Expr); }
		public AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR(Index), ast.Cast<uint>(Expr)); }


		private delegate void* AddressToPointerFunc( uint Address );

		public static AstNodeExpr AstMemoryGetPointer(AstNodeExpr Address, bool Safe, string ErrorDescription = "ERROR")
		{
			if (Safe)
			{
				return ast.CallInstance(
					CpuThreadStateArgument(),
					(AddressToPointerFunc)CpuThreadState.Methods.GetMemoryPtrSafe,
					ast.Cast<uint>(Address)
				);
			}
			else
			{
				return ast.CallInstance(
					CpuThreadStateArgument(),
					(AddressToPointerFunc)CpuThreadState.Methods.GetMemoryPtr,
					ast.Cast<uint>(Address)
				);
			}
		}
	}
}
