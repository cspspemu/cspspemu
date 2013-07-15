using CSPspEmu.Core.Cpu.InstructionCache;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public enum InvalidAddressAsEnum
	{
		Null = 0,
		InvalidAddress = 1,
		Exception = 2,
	}

	unsafe public class AstMipsGenerator : AstGenerator
	{
		static public readonly new AstMipsGenerator Instance = new AstMipsGenerator();
		static private readonly AstMipsGenerator ast = Instance;

		public AstNodeExprCallDelegate MethodCacheInfoCallStaticPC(CpuProcessor CpuProcessor, uint PC)
		{
			if (_DynarecConfig.FunctionCallWithStaticReferences)
			{
				var MethodCacheInfo = CpuProcessor.MethodCache.GetForPC(PC);
				return ast.CallDelegate(ast.StaticFieldAccess(MethodCacheInfo.StaticField.FieldInfo), ast.CpuThreadState);
			}
			else
			{
				return ast.CallDelegate(ast.CallInstance(ast.CpuThreadState, (Func<uint, Action<CpuThreadState>>)CpuThreadStateMethods.GetFuncAtPC, PC), ast.CpuThreadState);
			}
		}

		public AstNodeExprCall MethodCacheInfoCallDynamicPC(AstNodeExpr PC)
		{
			if (_DynarecConfig.FunctionCallWithStaticReferences)
			{
				return ast.CallInstance(GetMethodCacheInfoAtPC(PC), (Action<CpuThreadState>)MethodCacheInfo.Methods.CallDelegate, ast.CpuThreadState);
			}
			else
			{
				return ast.CallDelegate(ast.CallInstance(ast.CpuThreadState, (Func<uint, Action<CpuThreadState>>)CpuThreadStateMethods.GetFuncAtPC, PC), ast.CpuThreadState);
			}
		}

		public AstNodeExpr GetMethodCacheInfoAtPC(AstNodeExpr PC)
		{
			return ast.CallInstance(ast.FieldAccess(ast.CpuThreadState, "MethodCache"), (Func<uint, MethodCacheInfo>)MethodCache.Methods.GetForPC, PC);
		}

		public AstNodeExprArgument CpuThreadState { get { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); } }

		//private AstNodeExprArgument GetCpuThreadStateArgument() { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); }
		public AstNodeExprLValue FCR31_CC() { return ast.FieldAccess(REG("Fcr31"), "CC"); }
		public AstNodeExprLValue REG(string RegName) { return ast.FieldAccess(ast.CpuThreadState, RegName); }
		public AstNodeExprLValue FPR(int Index) { return REG("FPR" + Index); }
		public AstNodeExprLValue HI_LO() { return ast.PropertyAccess(ast.CpuThreadState, "HI_LO"); }
		public AstNodeExprLValue FPR_I(int Index) { return ast.Indirect(ast.Cast(typeof(int*), ast.GetAddress(REG("FPR" + Index)), Explicit: false)); }
		public AstNodeExprLValue GPR_F(int Index) { return ast.Indirect(ast.Cast(typeof(float*), ast.GetAddress(REG("GPR" + Index)), Explicit: false)); }

		public AstNodeStm AssignFPR_F(int Index, AstNodeExpr Expr) { return ast.Assign(ast.FPR(Index), Expr); }
		public AstNodeStm AssignFPR_I(int Index, AstNodeExpr Expr) { return ast.Assign(ast.FPR_I(Index), Expr); }
		public AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(ast.REG(RegName), Expr); }
		public AstNodeStm AssignHILO(AstNodeExpr Expr) { return ast.Assign(HI_LO(), ast.Cast<long>(Expr)); }
		public AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR(Index), ast.Cast<uint>(Expr, false)); }
		public AstNodeStm AssignGPR_F(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR_F(Index), ast.Cast<float>(Expr, false)); }
		//public AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(REG(RegName), Expr); }
		//public AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR(Index), ast.Cast<uint>(Expr)); }

		static private CpuThreadState CpuThreadStateMethods = CSPspEmu.Core.Cpu.CpuThreadState.Methods;

		public AstNodeExprLValue PrefixSource() { return ast.FieldAccess(ast.CpuThreadState, ILFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource)); }
		public AstNodeExprLValue PrefixSourceEnabled() { return ast.FieldAccess(PrefixSource(), ILFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource.Enabled)); }

		public AstNodeExprLValue PrefixDestination() { return ast.FieldAccess(ast.CpuThreadState, ILFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource)); }
		public AstNodeExprLValue PrefixDestinationEnabled() { return ast.FieldAccess(PrefixDestination(), ILFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixDestination.Enabled)); }

		public AstNodeExprLValue VCC(int Index)
		{
			return REG("VFR_CC_" + Index);
		}

		public AstNodeStm AssignVCC(int Index, AstNodeExpr Expr)
		{
			return ast.Assign(VCC(Index), Expr);
		}

		public AstNodeExprLValue GPR(int Index) { if (Index == 0) throw (new Exception("Can't get reference to GPR0")); return REG("GPR" + Index); }
		public AstNodeExprLValue GPR_l(int Index) { return ast.Indirect(ast.Cast(typeof(long*), ast.GetAddress(GPR(Index)))); }
		public AstNodeExpr GPR_f(int Index) { if (Index == 0) return ast.Immediate((int)0); return ast.Reinterpret<float>(GPR(Index)); }
		public AstNodeExpr GPR_s(int Index) { if (Index == 0) return ast.Immediate((int)0); return ast.Cast<int>(GPR(Index), Explicit: false); }
		public AstNodeExpr GPR_sl(int Index) { return ast.Cast<long>(GPR_s(Index)); }
		public AstNodeExpr GPR_u(int Index) { if (Index == 0) return ast.Immediate((uint)0); return ast.Cast<uint>(GPR(Index), Explicit: false); }
		public AstNodeExpr GPR_ul(int Index) { return ast.Cast<ulong>(GPR_u(Index)); }
		public AstNodeExpr HILO_sl() { return HI_LO(); }
		public AstNodeExpr HILO_ul() { return ast.Cast<ulong>(HILO_sl()); }

		private delegate void* AddressToPointerWithErrorFunc(uint Address, string ErrorDescription, bool CanBeNull, InvalidAddressAsEnum Invalid);
		private delegate void* AddressToPointerFunc(uint Address);

		public AstNodeExpr MemoryGetPointer(PspMemory Memory, AstNodeExpr Address, bool Safe, string ErrorDescription = "ERROR", InvalidAddressAsEnum InvalidAddress = InvalidAddressAsEnum.Exception)
		{
			if (Safe)
			{
				return ast.CallInstance(
					ast.CpuThreadState,
					(AddressToPointerWithErrorFunc)CSPspEmu.Core.Cpu.CpuThreadState.Methods.GetMemoryPtrSafeWithError,
					ast.Cast<uint>(Address),
					ErrorDescription,
					true,
					ast.Immediate(InvalidAddress)
				);
			}
			else
			{
				if (_DynarecConfig.AllowFastMemory && Memory.HasFixedGlobalAddress)
				{
					var AddressMasked = ast.Binary(Address, "&", ast.Immediate(PspMemory.MemoryMask));
					return ast.Immediate(Memory.FixedGlobalAddress) + AddressMasked;
				}
				else
				{
					return ast.CallInstance(
						ast.CpuThreadState,
						(AddressToPointerFunc)CSPspEmu.Core.Cpu.CpuThreadState.Methods.GetMemoryPtr,
						Address
					);
				}
			}
		}

		public AstNodeExpr MemoryGetPointer(PspMemory Memory, AstNodeExpr Address)
		{
			return MemoryGetPointer(Memory, Address, false);
		}

		public AstNodeExprLValue MemoryGetPointerRef(Type Type, PspMemory Memory, AstNodeExpr Address)
		{
			return ast.Indirect(ast.Cast(Type.MakePointerType(), MemoryGetPointer(Memory, Address), false));
		}

		public AstNodeExprLValue MemoryGetPointerRef<TType>(PspMemory Memory, AstNodeExpr Address)
		{
			return MemoryGetPointerRef(typeof(TType), Memory, Address);
		}

		public AstNodeStm MemorySetValue(Type Type, PspMemory Memory, AstNodeExpr Address, AstNodeExpr Value)
		{
			return ast.Assign(
				MemoryGetPointerRef(Type, Memory, Address),
				ast.Cast(Type, Value, false)
			);
		}

		public AstNodeStm MemorySetValue<T>(PspMemory Memory, AstNodeExpr Address, AstNodeExpr Value)
		{
			return MemorySetValue(typeof(T), Memory, Address, Value);
		}

		public AstNodeExpr MemoryGetValue(Type Type, PspMemory Memory, AstNodeExpr Address)
		{
			return MemoryGetPointerRef(Type, Memory, Address);
		}

		public AstNodeExpr MemoryGetValue<T>(PspMemory Memory, AstNodeExpr Address)
		{
			return MemoryGetValue(typeof(T), Memory, Address);
		}

		public AstNodeStm GetTickCall()
		{
			if (_DynarecConfig.EMIT_CALL_TICK)
			{
				return ast.Statement(ast.CallInstance(ast.CpuThreadState, (Action)CSPspEmu.Core.Cpu.CpuThreadState.Methods.Tick));
			}
			else
			{
				return ast.Statement();
			}
		}

		static public void ErrorWriteLine(string Line)
		{
			Console.Error.WriteLine(Line);
		}

		public AstNodeStm NotImplemented(
			[CallerMemberName]string sourceMemberName = "",
			[CallerFilePath]string sourceFilePath = "",
			[CallerLineNumber]int sourceLineNo = 0)
		{
			var Description = String.Format("('{0}') : {1}:{2}", sourceMemberName, Path.GetFileName(sourceFilePath), sourceLineNo);
			return ast.Statement(ast.CallStatic((Action<string>)ErrorWriteLine, "AstNotImplemented: " + Description));
		}
	}
}
