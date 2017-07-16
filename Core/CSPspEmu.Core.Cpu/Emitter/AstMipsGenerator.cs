using CSPspEmu.Core.Cpu.InstructionCache;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

		public AstNodeExprCallDelegate MethodCacheInfoCallStaticPC(CpuProcessor cpuProcessor, uint pc)
		{
			if (_DynarecConfig.FunctionCallWithStaticReferences)
			{
				var MethodCacheInfo = cpuProcessor.MethodCache.GetForPC(pc);
				return ast.CallDelegate(ast.StaticFieldAccess(MethodCacheInfo.StaticField.FieldInfo), ast.CpuThreadState);
			}
			else
			{
				return ast.CallDelegate(ast.CallInstance(ast.CpuThreadState, (Func<uint, Action<CpuThreadState>>)CpuThreadStateMethods.GetFuncAtPC, pc), ast.CpuThreadState);
			}
		}

		public AstNodeStm MethodCacheInfoCallDynamicPC(AstNodeExpr pc, bool tailCall)
		{
			//if (_DynarecConfig.FunctionCallWithStaticReferences)
			//{
			//	var Call = (AstNodeExpr)ast.CallInstance(GetMethodCacheInfoAtPC(PC), (Action<CpuThreadState>)MethodCacheInfo.Methods.CallDelegate, ast.CpuThreadState);
			//	if (TailCall) Call = ast.TailCall(Call as AstNodeExprCall);
			//	return ast.Statement(Call);
			//}
			//else
			{
				var LocalCachedPC = ast.Local(AstLocal.Create<uint>("CachedPC"));
				var LocalCachedFunction = ast.Local(AstLocal.Create<Action<CpuThreadState>>("CachedFunction"));
				var LocalCalculatePC = ast.Local(AstLocal.Create<uint>("CalculatePC"));
	
				var Call = (AstNodeExpr)ast.CallDelegate(LocalCachedFunction, ast.CpuThreadState);
				var CallStm = (AstNodeStm)ast.Statement(Call);
				if (tailCall) CallStm = ast.Statements(ast.Statement(ast.TailCall(Call as AstNodeExprCall)), ast.Return());

				return ast.Statements(
					ast.Assign(LocalCalculatePC, pc),
					ast.If
						(ast.Binary(LocalCachedPC, "!=", LocalCalculatePC),
						ast.Statements(
							ast.Assign(LocalCachedPC, LocalCalculatePC),
							ast.Assign(LocalCachedFunction, ast.CallInstance(ast.CpuThreadState, (Func<uint, Action<CpuThreadState>>)CpuThreadStateMethods.GetFuncAtPC, pc))
						)
					),
					CallStm
				);
			}
		}
		static readonly FieldInfo CpuThreadState_MethodCache_FieldInfo = typeof(CSPspEmu.Core.Cpu.CpuThreadState).GetField("MethodCache");

		public AstNodeExpr GetMethodCacheInfoAtPC(AstNodeExpr PC)
		{
			return ast.CallInstance(ast.FieldAccess(ast.CpuThreadState, CpuThreadState_MethodCache_FieldInfo), (Func<uint, MethodCacheInfo>)MethodCache.Methods.GetForPC, PC);
		}

		public AstNodeExprArgument CpuThreadState { get { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); } }

		//private AstNodeExprArgument GetCpuThreadStateArgument() { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); }
		public AstNodeExprLValue FCR31_CC() { return ast.FieldAccess(REG("Fcr31"), "CC"); }

		static private readonly Dictionary<string, AstNodeExprFieldAccess> _REG_Cache = new Dictionary<string, AstNodeExprFieldAccess>(1024);

		public AstNodeExprLValue BranchFlag()
		{
			return REG("BranchFlag");
		}

		private AstNodeExprLValue REG(string RegName)
		{
#if true
			if (!_REG_Cache.ContainsKey(RegName))
			{
				var FieldInfo = CpuThreadStateType.GetField(RegName);
				_REG_Cache[RegName] = ast.FieldAccess(ast.CpuThreadState, FieldInfo, RegName);
			}
			return _REG_Cache[RegName];
#else
			return ast.FieldAccess(ast.CpuThreadState, RegName);
#endif
		}

		static private readonly AstNodeExprLValue[] _GPR_Cache = new AstNodeExprLValue[32];

		static AstMipsGenerator()
		{
			for (int n = 0; n < 32; n++)
			{
				_GPR_Cache[n] = ast.FieldAccess(ast.CpuThreadState, CpuThreadStateType.GetField("GPR" + n));
			}
		}

		private AstNodeExprLValue RefGPRIndex(int Index)
		{
			return _GPR_Cache[Index];
		}
		public AstNodeExprLValue FPR(int index) { return REG("FPR" + index); }
		public AstNodeExprLValue HI_LO() { return ast.PropertyAccess(ast.CpuThreadState, "HI_LO"); }
		public AstNodeExprLValue FPR_I(int index) { return ast.Indirect(ast.Cast(typeof(int*), ast.GetAddress(REG("FPR" + index)), Explicit: false)); }
		public AstNodeExprLValue GPR_F(int index) { return ast.Indirect(ast.Cast(typeof(float*), ast.GetAddress(RefGPRIndex(index)), Explicit: false)); }

		public AstNodeStm AssignFPR_F(int index, AstNodeExpr expr) { return ast.Assign(ast.FPR(index), expr); }
		public AstNodeStm AssignFPR_I(int index, AstNodeExpr expr) { return ast.Assign(ast.FPR_I(index), expr); }
		private AstNodeStm AssignREG(string regName, AstNodeExpr expr) { return ast.Assign(ast.REG(regName), expr); }
		public AstNodeStm AssignPC(AstNodeExpr expr) { return ast.Assign(ast.PC(), expr); }
		public AstNodeStm AssignHI(AstNodeExpr expr) { return ast.Assign(ast.HI(), expr); }
		public AstNodeStm AssignLO(AstNodeExpr expr) { return ast.Assign(ast.LO(), expr); }
		public AstNodeStm AssignIC(AstNodeExpr expr) { return ast.Assign(ast.IC(), expr); }
		public AstNodeStm AssignC0R(int index, AstNodeExpr expr) { return ast.Assign(ast.C0R(index), expr); }
		public AstNodeStm AssignHILO(AstNodeExpr expr) { return ast.Assign(HI_LO(), ast.Cast<long>(expr)); }
		public AstNodeStm AssignGPR(int index, AstNodeExpr expr) { if (index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR(index), ast.Cast<uint>(expr, false)); }
		public AstNodeStm AssignGPR_F(int index, AstNodeExpr expr) { if (index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR_F(index), ast.Cast<float>(expr, false)); }
		//public AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(REG(RegName), Expr); }
		//public AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR(Index), ast.Cast<uint>(Expr)); }

		static readonly private CpuThreadState CpuThreadStateMethods = CSPspEmu.Core.Cpu.CpuThreadState.Methods;
		static readonly private Type CpuThreadStateType = typeof(CSPspEmu.Core.Cpu.CpuThreadState);

		public AstNodeExprLValue PrefixSource() { return ast.FieldAccess(ast.CpuThreadState, ILFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource)); }
		public AstNodeExprLValue PrefixSourceEnabled() { return ast.FieldAccess(PrefixSource(), ILFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource.Enabled)); }

		public AstNodeExprLValue PrefixDestination() { return ast.FieldAccess(ast.CpuThreadState, ILFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource)); }
		public AstNodeExprLValue PrefixDestinationEnabled() { return ast.FieldAccess(PrefixDestination(), ILFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixDestination.Enabled)); }

		public AstNodeExprLValue VCC(int index)
		{
			return REG("VFR_CC_" + index);
		}

		public AstNodeExprLValue VFR(int index)
		{
			return ast.REG("VFR" + index);
		}


		public AstNodeStm AssignVCC(int index, AstNodeExpr expr)
		{
			return ast.Assign(VCC(index), expr);
		}

		public AstNodeExprLValue IC() { return REG("IC"); }
		public AstNodeExprLValue PC() { return REG("PC"); }
		public AstNodeExprLValue HI() { return REG("HI"); }
		public AstNodeExprLValue LO() { return REG("LO"); }
		public AstNodeExprLValue C0R(int index) { return REG("C0R" + index); }
		public AstNodeExprLValue GPR(int index) { if (index == 0) throw (new Exception("Can't get reference to GPR0")); return RefGPRIndex(index); }
		public AstNodeExprLValue GPR_l(int index) { return ast.Indirect(ast.Cast(typeof(long*), ast.GetAddress(GPR(index)))); }
		public AstNodeExpr GPR_f(int index) { if (index == 0) return ast.Immediate((float)0); return ast.Reinterpret<float>(GPR(index)); }
		public AstNodeExpr GPR_s(int index) { if (index == 0) return ast.Immediate((int)0); return ast.Cast<int>(GPR(index), Explicit: false); }
		public AstNodeExpr GPR_sl(int index) { return ast.Cast<long>(GPR_s(index)); }
		public AstNodeExpr GPR_u(int index) { if (index == 0) return ast.Immediate((uint)0); return GPR(index); }
		public AstNodeExpr GPR_ul(int index) { return ast.Cast<ulong>(GPR_u(index)); }
		public AstNodeExpr GPR<TType>(int index)  { return GPR(typeof(TType), index); }
		public AstNodeExpr GPR(Type type, int index)
		{
			if (type == typeof(int)) return GPR_s(index);
			if (type == typeof(uint)) return GPR_u(index);
			if (type == typeof(long)) return GPR_sl(index);
			if (type == typeof(ulong)) return GPR_ul(index);
			throw (new NotImplementedException("Invalid GPR type " + type + "!"));
		}
		public AstNodeExpr HILO_sl() { return HI_LO(); }
		public AstNodeExpr HILO_ul() { return ast.Cast<ulong>(HILO_sl()); }

		private delegate void* AddressToPointerWithErrorFunc(uint address, string errorDescription, bool canBeNull, InvalidAddressAsEnum invalid);
		private delegate void* AddressToPointerFunc(uint address);

		public AstNodeExpr MemoryGetPointer(PspMemory memory, AstNodeExpr address, bool safe, string errorDescription = "ERROR", InvalidAddressAsEnum invalidAddress = InvalidAddressAsEnum.Exception)
		{
			if (safe)
			{
				return ast.CallInstance(
					ast.CpuThreadState,
					(AddressToPointerWithErrorFunc)CSPspEmu.Core.Cpu.CpuThreadState.Methods.GetMemoryPtrSafeWithError,
					ast.Cast<uint>(address),
					errorDescription,
					true,
					ast.Immediate(invalidAddress)
				);
			}
			else
			{
				if (_DynarecConfig.AllowFastMemory && memory.HasFixedGlobalAddress)
				{
					if (_DynarecConfig.EnableFastPspMemoryUtilsGetFastMemoryReader)
					{
						return ast.CallStatic(FastPspMemoryUtils.GetFastMemoryReader(memory.FixedGlobalAddress), address);
					}
					else
					{
						var AddressMasked = ast.Binary(address, "&", ast.Immediate(FastPspMemory.FastMemoryMask));
						return ast.Immediate(memory.FixedGlobalAddress) + AddressMasked;
					}
				}
				else
				{
					return ast.CallInstance(
						ast.CpuThreadState,
						(AddressToPointerFunc)CSPspEmu.Core.Cpu.CpuThreadState.Methods.GetMemoryPtr,
						address
					);
				}
			}
		}

		public AstNodeExpr MemoryGetPointer(PspMemory memory, AstNodeExpr address)
		{
			return MemoryGetPointer(memory, address, false);
		}

		public AstNodeExprLValue MemoryGetPointerRef(Type type, PspMemory memory, AstNodeExpr address)
		{
			return ast.Indirect(ast.Cast(type.MakePointerType(), MemoryGetPointer(memory, address), false));
		}

		public AstNodeExprLValue MemoryGetPointerRef<TType>(PspMemory memory, AstNodeExpr address)
		{
			return MemoryGetPointerRef(typeof(TType), memory, address);
		}

		public AstNodeStm MemorySetValue(Type type, PspMemory memory, AstNodeExpr address, AstNodeExpr value)
		{
			return ast.Assign(
				MemoryGetPointerRef(type, memory, address),
				ast.Cast(type, value, false)
			);
		}

		public AstNodeStm MemorySetValue<T>(PspMemory memory, AstNodeExpr address, AstNodeExpr value)
		{
			return MemorySetValue(typeof(T), memory, address, value);
		}

		public AstNodeExpr MemoryGetValue(Type type, PspMemory memory, AstNodeExpr address)
		{
			return MemoryGetPointerRef(type, memory, address);
		}

		public AstNodeExpr MemoryGetValue<T>(PspMemory memory, AstNodeExpr address)
		{
			return MemoryGetValue(typeof(T), memory, address);
		}

		public AstNodeStm GetTickCall(bool mandatory)
		{
			mandatory = true;
			if (mandatory || _DynarecConfig.EmitCallTick)
			{
				return ast.Statement(ast.CallInstance(ast.CpuThreadState, (Action)CSPspEmu.Core.Cpu.CpuThreadState.Methods.Tick));
			}
			else
			{
				return ast.Statement();
			}
		}

		static public void ErrorWriteLine(string line)
		{
			Console.Error.WriteLine(line);
		}

		public AstNodeStm NotImplemented(
			[CallerMemberName] string sourceMemberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNo = 0)
		{
			var description = $"('{sourceMemberName}') : {Path.GetFileName(sourceFilePath)}:{sourceLineNo}";
			//throw(new NotImplementedException(String.Format("AstNotImplemented: {0}", Description)));
			return ast.Statement(ast.CallStatic((Action<string>)ErrorWriteLine, "AstNotImplemented: " + description));
		}
	}
}
