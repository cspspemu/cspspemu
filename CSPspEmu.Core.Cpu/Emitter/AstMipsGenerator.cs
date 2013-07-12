#define ALLOW_FAST_MEMORY
#define EMIT_CALL_TICK

using CSPspEmu.Core.Cpu.InstructionCache;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.Emitter
{
	unsafe public class AstMipsGenerator : AstGenerator
	{
		static public readonly new AstMipsGenerator Instance = new AstMipsGenerator();
		static private readonly AstMipsGenerator ast = Instance;

		public AstNodeExprCallDelegate MethodCacheInfoCallStaticPC(CpuProcessor CpuProcessor, uint PC)
		{
			var MethodCacheInfo = CpuProcessor.MethodCache.GetForPC(PC);
			return ast.CallDelegate(ast.StaticFieldAccess(MethodCacheInfo.StaticField.FieldInfo), CpuThreadStateArgument());
		}

		public AstNodeExprCall MethodCacheInfoCallDynamicPC(AstNodeExpr PC)
		{
			return ast.CallInstance(GetMethodCacheInfoAtPC(PC), (Action<CpuThreadState>)MethodCacheInfo.Methods.CallDelegate, CpuThreadStateArgument());
		}

		public AstNodeExpr GetMethodCacheInfoAtPC(AstNodeExpr PC)
		{
			return ast.CallInstance(ast.FieldAccess(ast.CpuThreadStateArgument(), "MethodCache"), (Func<uint, MethodCacheInfo>)MethodCache.Methods.GetForPC, PC);
		}

		public AstNodeExprArgument CpuThreadStateArgument() { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); }
		public AstNodeExprLValue FCR31_CC() { return ast.FieldAccess(REG("Fcr31"), "CC"); }
		public AstNodeExprLValue REG(string RegName) { return ast.FieldAccess(ast.CpuThreadStateArgument(), RegName); }
		public AstNodeExprLValue FPR(int Index) { return REG("FPR" + Index); }
		public AstNodeExprLValue FPR_I(int Index) { return ast.Indirect(ast.Cast(typeof(int*), ast.GetAddress(REG("FPR" + Index)), Explicit: false)); }

		public AstNodeStm AssignFPR_F(int Index, AstNodeExpr Expr) { return ast.Assign(ast.FPR(Index), Expr); }
		public AstNodeStm AssignFPR_I(int Index, AstNodeExpr Expr) { return ast.Assign(ast.FPR_I(Index), Expr); }
		public AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(ast.REG(RegName), Expr); }
		public AstNodeStm AssignGPR(int Index, AstNodeExpr Expr)
		{
			if (Index == 0) return new AstNodeStmEmpty();
			return ast.Assign(GPR(Index), ast.Cast<uint>(Expr, false));
		}
		//public AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(REG(RegName), Expr); }
		//public AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR(Index), ast.Cast<uint>(Expr)); }

		public AstNodeStm AssignHILO(AstNodeExpr Expr)
		{
			return ast.Statement(ast.CallStatic(
				(Action<CpuThreadState, long>)CpuEmitterUtils._assign_hi_lo_impl,
				CpuThreadStateArgument(),
				ast.Cast<long>(Expr)
			));
		}

		public AstNodeExprLValue GPR(int Index) { if (Index == 0) throw (new Exception("Can't get reference to GPR0")); return REG("GPR" + Index); }
		public AstNodeExprLValue GPR_l(int Index) { return ast.Indirect(ast.Cast(typeof(long*), ast.GetAddress(GPR(Index)))); }
		public AstNodeExpr GPR_f(int Index) { if (Index == 0) return ast.Immediate((int)0); return ast.Reinterpret<float>(GPR(Index)); }
		public AstNodeExpr GPR_s(int Index) { if (Index == 0) return ast.Immediate((int)0); return ast.Cast<int>(GPR(Index)); }
		public AstNodeExpr GPR_sl(int Index) { return ast.Cast<long>(GPR_s(Index)); }
		public AstNodeExpr GPR_u(int Index) { if (Index == 0) return ast.Immediate((uint)0); return ast.Cast<uint>(GPR(Index)); }
		public AstNodeExpr GPR_ul(int Index) { return ast.Cast<ulong>(GPR_u(Index)); }
		//public AstNodeExpr IMM_s() { return this.Immediate(IMM); }
		//public AstNodeExpr IMM_u() { return this.Immediate((uint)(ushort)IMM); }
		//public AstNodeExpr IMM_uex() { return this.Immediate((uint)IMM); }



		private delegate void* AddressToPointerFunc(uint Address);
		//delegate void* AddressToPointerFunc(uint Address);


		public AstNodeExpr AstMemoryGetPointer(PspMemory Memory, AstNodeExpr Address, bool Safe, string ErrorDescription = "ERROR")
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
#if ALLOW_FAST_MEMORY
				if (Memory.HasFixedGlobalAddress)
				{
					var AddressMasked = ast.Binary(Address, "&", ast.Immediate(PspMemory.MemoryMask));
					return ast.Immediate(Memory.FixedGlobalAddress) + AddressMasked;
				}
				else
#endif
				{
					return ast.CallInstance(
						ast.CpuThreadStateArgument(),
						(AddressToPointerFunc)CpuThreadState.Methods.GetMemoryPtr,
						Address
					);
				}
			}
		}


		public AstNodeExpr AstMemoryGetPointer(PspMemory Memory, AstNodeExpr Address)
		{
			return AstMemoryGetPointer(Memory, Address, false);
		}

		public AstNodeExprIndirect AstMemoryGetPointerIndirect(PspMemory Memory, Type Type, AstNodeExpr Address)
		{
			return ast.Indirect(ast.Cast(Type.MakePointerType(), AstMemoryGetPointer(Memory, Address), false));
		}

		public AstNodeStm AstMemorySetValue(PspMemory Memory, Type Type, AstNodeExpr Address, AstNodeExpr Value)
		{
#if ALLOW_FAST_MEMORY
			if (Memory.HasFixedGlobalAddress)
			{
				return ast.Assign(
					AstMemoryGetPointerIndirect(Memory, Type, Address),
					ast.Cast(Type, Value, false)
				);
			}
			else
#endif
			{
				var SignedType = AstUtils.GetSignedType(Type);
				if (false) { }
				else if (SignedType == typeof(sbyte)) return ast.Statement(ast.CallInstance(ast.CpuThreadStateArgument(), (Action<uint, byte>)CpuThreadState.Methods.Write1, Address, ast.Cast<byte>(Value, false)));
				else if (SignedType == typeof(short)) return ast.Statement(ast.CallInstance(ast.CpuThreadStateArgument(), (Action<uint, ushort>)CpuThreadState.Methods.Write2, Address, ast.Cast<ushort>(Value, false)));
				else if (SignedType == typeof(int)) return ast.Statement(ast.CallInstance(ast.CpuThreadStateArgument(), (Action<uint, uint>)CpuThreadState.Methods.Write4, Address, ast.Cast<uint>(Value, false)));
				else if (SignedType == typeof(float)) return ast.Statement(ast.CallInstance(ast.CpuThreadStateArgument(), (Action<uint, float>)CpuThreadState.Methods.Write4F, Address, ast.Cast<float>(Value, false)));
				throw (new NotImplementedException(String.Format("Can't handle type {0}", Type)));
			}
		}

		public AstNodeStm AstMemorySetValue<T>(PspMemory Memory, AstNodeExpr Address, AstNodeExpr Value)
		{
			return AstMemorySetValue(Memory, typeof(T), Address, Value);
		}

		public AstNodeExpr AstMemoryGetValue(PspMemory Memory, Type Type, AstNodeExpr Address)
		{
#if ALLOW_FAST_MEMORY
			if (Memory.HasFixedGlobalAddress)
			{
				return AstMemoryGetPointerIndirect(Memory, Type, Address);
			}
			else
#endif
			{
				var SignedType = AstUtils.GetSignedType(Type);
				if (false) { }
				else if (SignedType == typeof(sbyte)) return ast.Cast(Type, ast.CallInstance(ast.CpuThreadStateArgument(), (Func<uint, byte>)CpuThreadState.Methods.Read1, Address), false);
				else if (SignedType == typeof(short)) return ast.Cast(Type, ast.CallInstance(ast.CpuThreadStateArgument(), (Func<uint, ushort>)CpuThreadState.Methods.Read2, Address), false);
				else if (SignedType == typeof(int)) return ast.Cast(Type, ast.CallInstance(ast.CpuThreadStateArgument(), (Func<uint, uint>)CpuThreadState.Methods.Read4, Address), false);
				else if (SignedType == typeof(float)) return ast.Cast(Type, ast.CallInstance(ast.CpuThreadStateArgument(), (Func<uint, float>)CpuThreadState.Methods.Read4F, Address), false);
				throw (new NotImplementedException(String.Format("Can't handle type {0}", Type)));
			}
		}

		public AstNodeExpr AstMemoryGetValue<T>(PspMemory Memory, AstNodeExpr Address)
		{
			return AstMemoryGetValue(Memory, typeof(T), Address);
		}

		public AstNodeStm GetTickCall()
		{
#if EMIT_CALL_TICK
			return ast.Statement(ast.CallInstance(ast.CpuThreadStateArgument(), (Action)CpuThreadState.Methods.Tick));
#else
			return ast.Statement();
#endif
		}

		public void ErrorWriteLine(string Line)
		{
			Console.Error.WriteLine(Line);
		}

		private AstNodeStm _AstNotImplemented(string Description)
		{
			//throw (new NotImplementedException("AstNotImplemented: " + Description));
			return ast.Statement(ast.CallStatic((Action<string>)ErrorWriteLine, "AstNotImplemented: " + Description));
		}

		public AstNodeStm AstNotImplemented(
			[CallerMemberName]string sourceMemberName = "",
			[CallerFilePath]string sourceFilePath = "",
			[CallerLineNumber]int sourceLineNo = 0)
		{
			return _AstNotImplemented(String.Format("('{0}') : {1}:{2}", sourceMemberName, Path.GetFileName(sourceFilePath), sourceLineNo));
		}

		//private AstNodeStm GenerateIL(AstNodeStm Expr) { MipsMethodEmitter.GenerateIL(Expr); return Expr; }
		//private void GenerateAssignREG(AstNodeExprLValue Reg, AstNodeExpr Expr) { GenerateIL(this.Assign(Reg, Expr)); }
		//private void GenerateAssignGPR(int Index, AstNodeExpr Expr) { GenerateIL(AssignGPR(Index, Expr)); }
		//private void GenerateAssignFPR_F(int Index, AstNodeExpr Expr) { GenerateIL(this.Assign(FPR(Index), Expr)); }
		//private void GenerateAssignFPR_I(int Index, AstNodeExpr Expr) { GenerateIL(this.Assign(FPR_I(Index), Expr)); }
		//private void GenerateAssignHILO(AstNodeExpr Expr)
		//{
		//	GenerateIL(this.Statement(this.CallStatic(
		//		(Action<CpuThreadState, long>)CpuEmitterUtils._assign_hi_lo_impl,
		//		this.CpuThreadStateArgument(),
		//		this.Cast<long>(Expr)
		//	)));
		//}

		protected AstMipsGenerator() : base()
		{
		}
	}
}
