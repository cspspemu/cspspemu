#define ALLOW_FAST_MEMORY

using System;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		public CpuProcessor CpuProcessor;
		private MipsMethodEmitter MipsMethodEmitter;
		private IInstructionReader InstructionReader;
		public Instruction Instruction { private set; get; }
		public uint PC { private set; get; }

		static private AstGenerator ast = AstGenerator.Instance;

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

		public CpuEmitter(MipsMethodEmitter MipsMethodEmitter, IInstructionReader InstructionReader, CpuProcessor CpuProcessor)
		{
			this.MipsMethodEmitter = MipsMethodEmitter;
			this.InstructionReader = InstructionReader;
			this.CpuProcessor = CpuProcessor;
		}

		// AST utilities
		private AstNodeExprArgument CpuThreadStateArgument() { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); }
		private AstNodeExprLValue FCR31_CC() { return ast.FieldAccess(REG("Fcr31"), "CC"); }
		private AstNodeExprLValue REG(string RegName) { return ast.FieldAccess(CpuThreadStateArgument(), RegName); }
		private AstNodeExprLValue GPR(int Index) { if (Index == 0) throw (new Exception("Can't get reference to GPR0")); return REG("GPR" + Index); }
		private AstNodeExprLValue FPR(int Index) { return REG("FPR" + Index); }
		private AstNodeExprLValue FPR_I(int Index) { return ast.Indirect(ast.Cast(typeof(int*), ast.GetAddress(REG("FPR" + Index)), Explicit: false)); }
		private AstNodeExpr GPR_s(int Index) { if (Index == 0) return ast.Immediate((int)0); return ast.Cast<int>(GPR(Index)); }
		private AstNodeExpr GPR_sl(int Index) { return ast.Cast<long>(GPR_s(Index)); }
		private AstNodeExpr GPR_u(int Index) { if (Index == 0) return ast.Immediate((uint)0); return ast.Cast<uint>(GPR(Index)); }
		private AstNodeExpr GPR_ul(int Index) { return ast.Cast<ulong>(GPR_u(Index)); }
		private AstNodeExpr IMM_s() { return ast.Immediate(IMM); }
		private AstNodeExpr IMM_u() { return ast.Immediate((uint)(ushort)IMM); }
		private AstNodeExpr IMM_uex() { return ast.Immediate((uint)IMM); }

		private AstNodeStm AssignFPR_F(int Index, AstNodeExpr Expr) { return ast.Assign(FPR(Index), Expr); }
		private AstNodeStm AssignFPR_I(int Index, AstNodeExpr Expr) { return ast.Assign(FPR_I(Index), Expr); }
		private AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(REG(RegName), Expr); }
		private AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR(Index), ast.Cast<uint>(Expr)); }
		private AstNodeStm AssignHILO(AstNodeExpr Expr)
		{
			return ast.Statement(ast.CallStatic(
				(Action<CpuThreadState, long>)CpuEmitterUtils._assign_hi_lo_impl,
				CpuThreadStateArgument(),
				ast.Cast<long>(Expr)
			));
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



		private AstNodeExpr HILO_sl() { return ast.CallStatic((Func<CpuThreadState, long>)CpuEmitterUtils._get_hi_lo_impl, this.CpuThreadStateArgument()); }
		private AstNodeExpr HILO_ul() { return ast.Cast<ulong>(HILO_sl()); }

		public AstNodeExpr Address_RS_IMM()
		{
			return ast.Cast<uint>(ast.Binary(GPR_s(RS), "+", IMM_s()));
		}

		delegate void* AddressToPointerFunc(uint Address);

		private AstNodeExpr AstMemoryGetPointer(AstNodeExpr Address)
		{
#if ALLOW_FAST_MEMORY
			var FastMemory = CpuProcessor.Memory as FastPspMemory;
			if (FastMemory != null)
			{
				var AddressMasked = ast.Binary(Address, "&", PspMemory.MemoryMask);
				return ast.Immediate(new IntPtr(FastMemory.Base)) + AddressMasked;
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

		private AstNodeExprIndirect AstMemoryGetPointerIndirect(Type Type, AstNodeExpr Address)
		{
			return ast.Indirect(ast.Cast(Type.MakePointerType(), AstMemoryGetPointer(Address)));
		}

		private AstNodeStm AstMemorySetValue(Type Type, AstNodeExpr Address, AstNodeExpr Value)
		{
#if ALLOW_FAST_MEMORY
			if (CpuProcessor.Memory is FastPspMemory)
			{
				return ast.Assign(
					AstMemoryGetPointerIndirect(Type, Address),
					ast.Cast(Type, Value)
				);
			}
			else
#endif
			{
				var SignedType = AstUtils.GetSignedType(Type);
				if (false) { }
				else if (SignedType == typeof(sbyte)) return ast.Statement(ast.CallInstance(this.CpuThreadStateArgument(), (Action<uint, byte>)CpuThreadState.Methods.Write1, Address, ast.Cast<byte>(Value)));
				else if (SignedType == typeof(short)) return ast.Statement(ast.CallInstance(this.CpuThreadStateArgument(), (Action<uint, ushort>)CpuThreadState.Methods.Write2, Address, ast.Cast<ushort>(Value)));
				else if (SignedType == typeof(int)) return ast.Statement(ast.CallInstance(this.CpuThreadStateArgument(), (Action<uint, uint>)CpuThreadState.Methods.Write4, Address, ast.Cast<uint>(Value)));
				throw (new NotImplementedException(String.Format("Can't handle type {0}", Type)));
			}
		}

		private AstNodeStm AstMemorySetValue<T>(AstNodeExpr Address, AstNodeExpr Value)
		{
			return AstMemorySetValue(typeof(T), Address, Value);
		}

		private unsafe AstNodeExpr AstMemoryGetValue(Type Type, AstNodeExpr Address)
		{
#if ALLOW_FAST_MEMORY
			if (CpuProcessor.Memory is FastPspMemory)
			{
				return AstMemoryGetPointerIndirect(Type, Address);
			}
			else
#endif
			{
				var SignedType = AstUtils.GetSignedType(Type);
				if (false) { }
				else if (SignedType == typeof(sbyte)) return ast.Cast(Type, ast.CallInstance(this.CpuThreadStateArgument(), (Func<uint, byte>)CpuThreadState.Methods.Read1, Address));
				else if (SignedType == typeof(short)) return ast.Cast(Type, ast.CallInstance(this.CpuThreadStateArgument(), (Func<uint, ushort>)CpuThreadState.Methods.Read2, Address));
				else if (SignedType == typeof(int)) return ast.Cast(Type, ast.CallInstance(this.CpuThreadStateArgument(), (Func<uint, uint>)CpuThreadState.Methods.Read4, Address));
				throw (new NotImplementedException(String.Format("Can't handle type {0}", Type)));
			}
		}

		private AstNodeExpr AstMemoryGetValue<T>(AstNodeExpr Address)
		{
			return AstMemoryGetValue(typeof(T), Address);
		}
	}
}
