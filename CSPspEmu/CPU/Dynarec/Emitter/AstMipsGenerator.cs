using CSPspEmu.Core.Cpu.InstructionCache;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public enum InvalidAddressAsEnum
    {
        Null = 0,
        InvalidAddress = 1,
        Exception = 2,
    }

    public unsafe class AstMipsGenerator : AstGenerator
    {
        public new static readonly AstMipsGenerator Instance = new AstMipsGenerator();
        private static readonly AstMipsGenerator Ast = Instance;

        public AstNodeExprCallDelegate MethodCacheInfoCallStaticPc(CpuProcessor cpuProcessor, uint pc)
        {
            if (DynarecConfig.FunctionCallWithStaticReferences)
            {
                var methodCacheInfo = cpuProcessor.MethodCache.GetForPc(pc);
                return Ast.CallDelegate(Ast.StaticFieldAccess(methodCacheInfo.StaticField.FieldInfo),
                    Ast.CpuThreadStateExpr);
            }
            else
            {
                return Ast.CallDelegate(
                    Ast.CallInstance(Ast.CpuThreadStateExpr,
                        (Func<uint, Action<CpuThreadState>>) CpuThreadStateMethods.GetFuncAtPc, pc),
                    Ast.CpuThreadStateExpr);
            }
        }

        public AstNodeStm MethodCacheInfoCallDynamicPc(AstNodeExpr pc, bool tailCall)
        {
            //if (_DynarecConfig.FunctionCallWithStaticReferences)
            //{
            //	var Call = (AstNodeExpr)ast.CallInstance(GetMethodCacheInfoAtPC(PC), (Action<CpuThreadState>)MethodCacheInfo.Methods.CallDelegate, ast.CpuThreadState);
            //	if (TailCall) Call = ast.TailCall(Call as AstNodeExprCall);
            //	return ast.Statement(Call);
            //}
            //else
            {
                var localCachedPc = Ast.Local(AstLocal.Create<uint>("CachedPC"));
                var localCachedFunction = Ast.Local(AstLocal.Create<Action<CpuThreadState>>("CachedFunction"));
                var localCalculatePc = Ast.Local(AstLocal.Create<uint>("CalculatePC"));

                var call = (AstNodeExpr) Ast.CallDelegate(localCachedFunction, Ast.CpuThreadStateExpr);
                var callStm = (AstNodeStm) Ast.Statement(call);
                if (tailCall)
                    callStm = Ast.Statements(Ast.Statement(Ast.TailCall((AstNodeExprCall) call)), Ast.Return());

                return Ast.Statements(
                    Ast.Assign(localCalculatePc, pc),
                    Ast.If
                    (Ast.Binary(localCachedPc, "!=", localCalculatePc),
                        Ast.Statements(
                            Ast.Assign(localCachedPc, localCalculatePc),
                            Ast.Assign(localCachedFunction,
                                Ast.CallInstance(Ast.CpuThreadStateExpr,
                                    (Func<uint, Action<CpuThreadState>>) CpuThreadStateMethods.GetFuncAtPc, pc))
                        )
                    ),
                    callStm
                );
            }
        }

        static readonly FieldInfo CpuThreadStateMethodCacheFieldInfo =
            typeof(CpuThreadState).GetField(nameof(CpuThreadState.MethodCache));

        public AstNodeExpr GetMethodCacheInfoAtPc(AstNodeExpr pc)
        {
            return Ast.CallInstance(Ast.FieldAccess(Ast.CpuThreadStateExpr, CpuThreadStateMethodCacheFieldInfo),
                (Func<uint, MethodCacheInfo>) MethodCache.Methods.GetForPc, pc);
        }

        public AstNodeExprArgument CpuThreadStateExpr => Ast.Argument<CpuThreadState>(0, "CpuThreadState");

        //private AstNodeExprArgument GetCpuThreadStateArgument() { return ast.Argument<CpuThreadState>(0, "CpuThreadState"); }
        public AstNodeExprLValue FCR31_CC()
        {
            return Ast.FieldAccess(Reg(nameof(CpuThreadState.Fcr31)), nameof(CpuThreadState.Fcr31Struct.Cc));
        }

        private static readonly Dictionary<string, AstNodeExprFieldAccess> RegCache =
            new Dictionary<string, AstNodeExprFieldAccess>(1024);

        public AstNodeExprLValue BranchFlag() => Reg(nameof(CpuThreadState.BranchFlag));

        private AstNodeExprLValue Reg(string regName)
        {
#if true
            if (RegCache.ContainsKey(regName)) return RegCache[regName];
            var fieldInfo = CpuThreadStateType.GetField(regName);
            RegCache[regName] = Ast.FieldAccess(Ast.CpuThreadStateExpr, fieldInfo, regName);
            return RegCache[regName];
#else
			return ast.FieldAccess(ast.CpuThreadState, RegName);
#endif
        }

        private static readonly AstNodeExprLValue[] GprCache = new AstNodeExprLValue[32];

        static AstMipsGenerator()
        {
            for (var n = 0; n < 32; n++)
                GprCache[n] = Ast.FieldAccess(Ast.CpuThreadStateExpr,
                    CpuThreadStateType.GetField(CpuThreadState.GprNames[n]));
        }

        private AstNodeExprLValue RefGprIndex(int index)
        {
            return GprCache[index];
        }

        public AstNodeExprLValue Fpr(int index) => Reg(CpuThreadState.FprNames[index]);

        public AstNodeExprLValue HI_LO() =>
            Ast.PropertyAccess(Ast.CpuThreadStateExpr, nameof(CpuThreadState.HiLo));

        public AstNodeExprLValue FPR_I(int index) =>
            Ast.Indirect(Ast.Cast(typeof(int*), Ast.GetAddress(Reg(CpuThreadState.FprNames[index])), Explicit: false));

        public AstNodeExprLValue GPR_F(int index) =>
            Ast.Indirect(Ast.Cast(typeof(float*), Ast.GetAddress(RefGprIndex(index)), Explicit: false));

        public AstNodeStm AssignFPR_F(int index, AstNodeExpr expr) => Ast.Assign(Ast.Fpr(index), expr);

        public AstNodeStm AssignFPR_I(int index, AstNodeExpr expr) => Ast.Assign(Ast.FPR_I(index), expr);

        //private AstNodeStm AssignReg(string regName, AstNodeExpr expr) => Ast.Assign(Ast.Reg(regName), expr);

        public AstNodeStm AssignPc(AstNodeExpr expr) => Ast.Assign(Ast.Pc(), expr);

        public AstNodeStm AssignHi(AstNodeExpr expr) => Ast.Assign(Ast.Hi(), expr);

        public AstNodeStm AssignLo(AstNodeExpr expr) => Ast.Assign(Ast.Lo, expr);

        public AstNodeStm AssignIc(AstNodeExpr expr) => Ast.Assign(Ast.Ic(), expr);

        public AstNodeStm AssignC0R(int index, AstNodeExpr expr) => Ast.Assign(Ast.C0R(index), expr);

        public AstNodeStm AssignHilo(AstNodeExpr expr) => Ast.Assign(HI_LO(), Ast.Cast<long>(expr));

        public AstNodeStm AssignGpr(int index, AstNodeExpr expr)
        {
            if (index == 0) return new AstNodeStmEmpty();
            return Ast.Assign(Gpr(index), Ast.Cast<uint>(expr, false));
        }

        public AstNodeStm AssignGPR_F(int index, AstNodeExpr expr)
        {
            if (index == 0) return new AstNodeStmEmpty();
            return Ast.Assign(GPR_F(index), Ast.Cast<float>(expr, false));
        }
        //public AstNodeStm AssignREG(string RegName, AstNodeExpr Expr) { return ast.Assign(REG(RegName), Expr); }
        //public AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return ast.Assign(GPR(Index), ast.Cast<uint>(Expr)); }

        private static readonly CpuThreadState CpuThreadStateMethods = CpuThreadState.Methods;
        private static readonly Type CpuThreadStateType = typeof(CpuThreadState);

        public AstNodeExprLValue PrefixSource() => Ast.FieldAccess(Ast.CpuThreadStateExpr,
            IlFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource));

        public AstNodeExprLValue PrefixSourceEnabled() => Ast.FieldAccess(PrefixSource(),
            IlFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource.Enabled));

        public AstNodeExprLValue PrefixDestination() => Ast.FieldAccess(Ast.CpuThreadStateExpr,
            IlFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixSource));

        public AstNodeExprLValue PrefixDestinationEnabled() => Ast.FieldAccess(PrefixDestination(),
            IlFieldInfo.GetFieldInfo(() => CpuThreadStateMethods.PrefixDestination.Enabled));

        public AstNodeExprLValue Vcc(int index) => Reg(CpuThreadState.VfrCcNames[index]);

        public AstNodeExprLValue Vfr(int index) => Ast.Reg(CpuThreadState.VfrNames[index]);

        public AstNodeStm AssignVcc(int index, AstNodeExpr expr) => Ast.Assign(Vcc(index), expr);

        public AstNodeExprLValue Ic() => Reg(nameof(CpuThreadState.Ic));

        public AstNodeExprLValue Pc() => Reg(nameof(CpuThreadState.Pc));

        public AstNodeExprLValue Hi() => Reg(nameof(CpuThreadState.Hi));

        public AstNodeExprLValue Lo => Reg(nameof(CpuThreadState.Lo));

        public AstNodeExprLValue C0R(int index) => Reg(CpuThreadState.C0RNames[index]);

        public AstNodeExprLValue Gpr(int index) =>
            index == 0 ? throw new Exception("Can't get reference to GPR0") : RefGprIndex(index);

        public AstNodeExprLValue GPR_l(int index) => Ast.Indirect(Ast.Cast(typeof(long*), Ast.GetAddress(Gpr(index))));

        public AstNodeExpr GPR_f(int index)
        {
            if (index == 0) return Ast.Immediate((float) 0);
            return Ast.Reinterpret<float>(Gpr(index));
        }

        public AstNodeExpr GPR_s(int index) =>
            index == 0 ? (AstNodeExpr) Ast.Immediate(0) : Ast.Cast<int>(Gpr(index), false);

        public AstNodeExpr GPR_sl(int index) => Ast.Cast<long>(GPR_s(index));
        public AstNodeExpr GPR_u(int index) => index == 0 ? (AstNodeExpr) Ast.Immediate((uint) 0) : Gpr(index);
        public AstNodeExpr GPR_ul(int index) => Ast.Cast<ulong>(GPR_u(index));
        public AstNodeExpr Gpr<TType>(int index) => Gpr(typeof(TType), index);

        public AstNodeExpr Gpr(Type type, int index)
        {
            if (type == typeof(int)) return GPR_s(index);
            if (type == typeof(uint)) return GPR_u(index);
            if (type == typeof(long)) return GPR_sl(index);
            if (type == typeof(ulong)) return GPR_ul(index);
            throw new NotImplementedException($"Invalid GPR type {type}!");
        }

        public AstNodeExpr HILO_sl() => HI_LO();

        public AstNodeExpr HILO_ul() => Ast.Cast<ulong>(HILO_sl());

        private delegate void* AddressToPointerWithErrorFunc(uint address, string errorDescription, bool canBeNull,
            InvalidAddressAsEnum invalid);

        private delegate void* AddressToPointerFunc(uint address);

        public AstNodeExpr MemoryGetPointer(PspMemory memory, AstNodeExpr address, bool safe,
            string errorDescription = "ERROR", InvalidAddressAsEnum invalidAddress = InvalidAddressAsEnum.Exception)
        {
            if (safe)
            {
                return Ast.CallInstance(
                    Ast.CpuThreadStateExpr,
                    (AddressToPointerWithErrorFunc) CpuThreadState.Methods.GetMemoryPtrSafeWithError,
                    Ast.Cast<uint>(address),
                    errorDescription,
                    true,
                    Ast.Immediate(invalidAddress)
                );
            }
            else
            {
                if (DynarecConfig.AllowFastMemory && memory.HasFixedGlobalAddress)
                {
                    if (DynarecConfig.EnableFastPspMemoryUtilsGetFastMemoryReader)
                    {
                        return Ast.CallStatic(FastPspMemoryUtils.GetFastMemoryReader(memory.FixedGlobalAddress),
                            address);
                    }
                    else
                    {
                        var addressMasked = Ast.Binary(address, "&", Ast.Immediate(FastPspMemory.FastMemoryMask));
                        return Ast.Immediate(memory.FixedGlobalAddress) + addressMasked;
                    }
                }
                else
                {
                    return Ast.CallInstance(
                        Ast.CpuThreadStateExpr,
                        (AddressToPointerFunc) CpuThreadState.Methods.GetMemoryPtr,
                        address
                    );
                }
            }
        }

        public AstNodeExpr MemoryGetPointer(PspMemory memory, AstNodeExpr address) =>
            MemoryGetPointer(memory, address, false);

        public AstNodeExprLValue MemoryGetPointerRef(Type type, PspMemory memory, AstNodeExpr address) =>
            Ast.Indirect(Ast.Cast(type.MakePointerType(), MemoryGetPointer(memory, address), false));

        public AstNodeExprLValue MemoryGetPointerRef<TType>(PspMemory memory, AstNodeExpr address) =>
            MemoryGetPointerRef(typeof(TType), memory, address);

        public AstNodeStm MemorySetValue(Type type, PspMemory memory, AstNodeExpr address, AstNodeExpr value) =>
            Ast.Assign(
                MemoryGetPointerRef(type, memory, address),
                Ast.Cast(type, value, false)
            );

        public AstNodeStm MemorySetValue<T>(PspMemory memory, AstNodeExpr address, AstNodeExpr value) =>
            MemorySetValue(typeof(T), memory, address, value);

        public AstNodeExpr MemoryGetValue(Type type, PspMemory memory, AstNodeExpr address) =>
            MemoryGetPointerRef(type, memory, address);

        public AstNodeExpr MemoryGetValue<T>(PspMemory memory, AstNodeExpr address) =>
            MemoryGetValue(typeof(T), memory, address);

        public AstNodeStm GetTickCall(bool mandatory)
        {
            //mandatory = true;
            if (mandatory || DynarecConfig.EmitCallTick)
            {
                return Ast.Statement(Ast.CallInstance(Ast.CpuThreadStateExpr,
                    (Action) CpuThreadState.Methods.Tick));
            }
            else
            {
                return Ast.Statement();
            }
        }

        public static void ErrorWriteLine(string line)
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
            return Ast.Statement(Ast.CallStatic((Action<string>) ErrorWriteLine, $"AstNotImplemented: {description}"));
        }
    }
}