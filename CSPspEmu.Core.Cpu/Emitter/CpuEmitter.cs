#define ALLOW_FAST_MEMORY
#define EMIT_CALL_TICK

using System;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System.Runtime.CompilerServices;
using System.IO;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		[Inject]
		public CpuProcessor CpuProcessor;

		private MipsMethodEmitter MipsMethodEmitter;

		private IInstructionReader InstructionReader;
		public Instruction Instruction { private set; get; }
		public uint PC { private set; get; }
		public PspMemory Memory { get { return CpuProcessor.Memory; } }

		static private AstMipsGenerator ast = AstMipsGenerator.Instance;

		public Instruction LoadAT(uint PC)
		{
			this.PC = PC;
			return this.Instruction = InstructionReader[PC];
		}

		public int RT { get { return Instruction.RT; } }
		public int RD { get { return Instruction.RD; } }
		public int RS { get { return Instruction.RS; } }
		public int IMM { get { return Instruction.IMM; } }
		public uint IMMU { get { return Instruction.IMMU; } }

		public int FT { get { return Instruction.FT; } }
		public int FD { get { return Instruction.FD; } }
		public int FS { get { return Instruction.FS; } }

		public CpuEmitter(InjectContext InjectContext, MipsMethodEmitter MipsMethodEmitter, IInstructionReader InstructionReader)
		{
			InjectContext.InjectDependencesTo(this);
			this.MipsMethodEmitter = MipsMethodEmitter;
			this.InstructionReader = InstructionReader;
		}

		private AstNodeStm GetTickCall()
		{
#if EMIT_CALL_TICK
			return ast.Statement(ast.CallInstance(MipsMethodEmitter.CpuThreadStateArgument(), (Action)CpuThreadState.Methods.Tick));
#else
			return ast.Statement();
#endif
		}

		// AST utilities
		public static AstNodeExprArgument CpuThreadStateArgument() { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); }
		public static AstNodeExprLValue FCR31_CC() { return ast.FieldAccess(REG("Fcr31"), "CC"); }
		public static AstNodeExprLValue REG(string RegName) { return ast.FieldAccess(CpuThreadStateArgument(), RegName); }
		public static AstNodeExprLValue GPR(int Index) { if (Index == 0) throw (new Exception("Can't get reference to GPR0")); return REG("GPR" + Index); }
		public static AstNodeExprLValue FPR(int Index) { return REG("FPR" + Index); }
		public static AstNodeExprLValue FPR_I(int Index) { return ast.Indirect(ast.Cast(typeof(int*), ast.GetAddress(REG("FPR" + Index)), Explicit: false)); }

		public static AstNodeExpr GPR_f(int Index) { if (Index == 0) return ast.Immediate((int)0); return ast.Reinterpret<float>(GPR(Index)); }
		public static AstNodeExpr GPR_s(int Index) { if (Index == 0) return ast.Immediate((int)0); return ast.Cast<int>(GPR(Index), false); }
		public static AstNodeExpr GPR_sl(int Index) { return ast.Cast<long>(GPR_s(Index)); }
		public static AstNodeExpr GPR_u(int Index) { if (Index == 0) return ast.Immediate((uint)0); return ast.Cast<uint>(GPR(Index), false); }
		public static AstNodeExpr GPR_ul(int Index) { return ast.Cast<ulong>(GPR_u(Index)); }

		public static AstNodeStm AssignFPR_F(int Index, AstNodeExpr Expr) { return ast.Assign(FPR(Index), Expr); }
		public static AstNodeStm AssignFPR_I(int Index, AstNodeExpr Expr) { return ast.Assign(FPR_I(Index), Expr); }
		public static AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(REG(RegName), Expr); }
		public static AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) {
			if (Index == 0) return new AstNodeStmEmpty();
			return ast.Assign(GPR(Index), ast.Cast<uint>(Expr, false));
		}
		public static AstNodeStm AssignHILO(AstNodeExpr Expr)
		{
			return ast.Statement(ast.CallStatic(
				(Action<CpuThreadState, long>)CpuEmitterUtils._assign_hi_lo_impl,
				CpuThreadStateArgument(),
				ast.Cast<long>(Expr)
			));
		}

		public static void ErrorWriteLine(string Line)
		{
			Console.Error.WriteLine(Line);
		}

		private static AstNodeStm _AstNotImplemented(string Description)
		{
			//throw (new NotImplementedException("AstNotImplemented: " + Description));
			return ast.Statement(ast.CallStatic((Action<string>)ErrorWriteLine, "AstNotImplemented: " + Description));
		}

		public static AstNodeStm AstNotImplemented(
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


		public AstNodeExpr IMM_s() { return ast.Immediate(IMM); }
		public AstNodeExpr IMM_u() { return ast.Immediate((uint)(ushort)IMM); }
		public AstNodeExpr IMM_uex() { return ast.Immediate((uint)IMM); }

		public AstNodeExpr HILO_sl() { return ast.CallStatic((Func<CpuThreadState, long>)CpuEmitterUtils._get_hi_lo_impl, CpuThreadStateArgument()); }
		public AstNodeExpr HILO_ul() { return ast.Cast<ulong>(HILO_sl()); }

		public AstNodeExpr Address_RS_IMM14(int Offset = 0)
		{
			return ast.Cast<uint>(ast.Binary(GPR_s(RS), "+", Instruction.IMM14 * 4 + Offset), false);
		}

		public AstNodeExpr Address_RS_IMM()
		{
			return ast.Cast<uint>(ast.Binary(GPR_s(RS), "+", IMM_s()), false);
		}

		delegate void* AddressToPointerFunc(uint Address);

		public static AstNodeExpr AstMemoryGetPointer(PspMemory Memory, AstNodeExpr Address)
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
					CpuThreadStateArgument(),
					(AddressToPointerFunc)CpuThreadState.Methods.GetMemoryPtr,
					Address
				);
			}
		}

		public static AstNodeExprIndirect AstMemoryGetPointerIndirect(PspMemory Memory, Type Type, AstNodeExpr Address)
		{
			return ast.Indirect(ast.Cast(Type.MakePointerType(), AstMemoryGetPointer(Memory, Address), false));
		}

		public static AstNodeStm AstMemorySetValue(PspMemory Memory, Type Type, AstNodeExpr Address, AstNodeExpr Value)
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
				else if (SignedType == typeof(sbyte)) return ast.Statement(ast.CallInstance(CpuThreadStateArgument(), (Action<uint, byte>)CpuThreadState.Methods.Write1, Address, ast.Cast<byte>(Value, false)));
				else if (SignedType == typeof(short)) return ast.Statement(ast.CallInstance(CpuThreadStateArgument(), (Action<uint, ushort>)CpuThreadState.Methods.Write2, Address, ast.Cast<ushort>(Value, false)));
				else if (SignedType == typeof(int)) return ast.Statement(ast.CallInstance(CpuThreadStateArgument(), (Action<uint, uint>)CpuThreadState.Methods.Write4, Address, ast.Cast<uint>(Value, false)));
				else if (SignedType == typeof(float)) return ast.Statement(ast.CallInstance(CpuThreadStateArgument(), (Action<uint, float>)CpuThreadState.Methods.Write4F, Address, ast.Cast<float>(Value, false)));
				throw (new NotImplementedException(String.Format("Can't handle type {0}", Type)));
			}
		}

		public static AstNodeStm AstMemorySetValue<T>(PspMemory Memory, AstNodeExpr Address, AstNodeExpr Value)
		{
			return AstMemorySetValue(Memory, typeof(T), Address, Value);
		}

		public static unsafe AstNodeExpr AstMemoryGetValue(PspMemory Memory, Type Type, AstNodeExpr Address)
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
				else if (SignedType == typeof(sbyte)) return ast.Cast(Type, ast.CallInstance(CpuThreadStateArgument(), (Func<uint, byte>)CpuThreadState.Methods.Read1, Address), false);
				else if (SignedType == typeof(short)) return ast.Cast(Type, ast.CallInstance(CpuThreadStateArgument(), (Func<uint, ushort>)CpuThreadState.Methods.Read2, Address), false);
				else if (SignedType == typeof(int)) return ast.Cast(Type, ast.CallInstance(CpuThreadStateArgument(), (Func<uint, uint>)CpuThreadState.Methods.Read4, Address), false);
				else if (SignedType == typeof(float)) return ast.Cast(Type, ast.CallInstance(CpuThreadStateArgument(), (Func<uint, float>)CpuThreadState.Methods.Read4F, Address), false);
				throw (new NotImplementedException(String.Format("Can't handle type {0}", Type)));
			}
		}

		public static AstNodeExpr AstMemoryGetValue<T>(PspMemory Memory, AstNodeExpr Address)
		{
			return AstMemoryGetValue(Memory, typeof(T), Address);
		}
	}
}
