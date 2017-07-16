using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;
using CSharpUtils;
using SafeILGenerator;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using SafeILGenerator.Ast.Optimizers;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using SafeILGenerator.Utils;

namespace CSPspEmu.Hle
{
    public unsafe partial class HleModuleHost : HleModule
    {
        [Inject] internal HleThreadManager ThreadManager;

        [Inject] internal CpuProcessor CpuProcessor;

        [Inject] internal HleConfig HleConfig;

        private IlInstanceHolderPoolItem _ThisILInstanceHolder = null;

        private IlInstanceHolderPoolItem ThisILInstanceHolder
        {
            get
            {
                if (_ThisILInstanceHolder == null)
                {
                    this._ThisILInstanceHolder = IlInstanceHolder.Alloc(this.GetType(), this);
                }
                return _ThisILInstanceHolder;
            }
        }

        static private readonly AstMipsGenerator ast = AstMipsGenerator.Instance;

        static public TType GetObjectFromPoolHelper<TType>(CpuThreadState CpuThreadState, int Index)
        {
            //Console.Error.WriteLine("GetObjectFromPoolHelper");
            return (TType) CpuThreadState.CpuProcessor.InjectContext.GetInstance<HleUidPoolManager>()
                .Get(typeof(TType), Index);
        }

        static public object GetObjectFromPoolHelper(CpuThreadState CpuThreadState, Type Type, int Index,
            bool CanReturnNull)
        {
            //Console.Error.WriteLine("GetObjectFromPoolHelper");
            return CpuThreadState.CpuProcessor.InjectContext.GetInstance<HleUidPoolManager>()
                .Get(Type, Index, CanReturnNull: CanReturnNull);
        }

        static public uint GetOrAllocIndexFromPoolHelper(CpuThreadState CpuThreadState, Type Type,
            IHleUidPoolClass Item)
        {
            //Console.Error.WriteLine("AllocIndexFromPoolHelper");
            return (uint) CpuThreadState.CpuProcessor.InjectContext.GetInstance<HleUidPoolManager>()
                .GetOrAllocIndex(Type, Item);
        }

        class NormalRegisterReader : RegisterReaderBase<object>
        {
            public CpuThreadState CpuThreadState;

            public NormalRegisterReader(CpuThreadState CpuThreadState)
            {
                this.CpuThreadState = CpuThreadState;
            }


            protected override object ReadFromFpr(Type Type, int Index)
            {
                return this.CpuThreadState.FPR[Index];
            }

            protected override object ReadFromGpr(Type Type, int Index)
            {
                return this.CpuThreadState.GPR[Index];
            }

            protected override object ReadFromStack(Type Type, int Index)
            {
                if (Type == typeof(int) || Type == typeof(uint))
                {
                    return CpuThreadState.Memory.ReadSafe<uint>(
                        (uint) (this.CpuThreadState.GPR[29] + ((MaxGprIndex - Index) * 4)));
                }
                if (Type == typeof(long) || Type == typeof(ulong))
                {
                    return CpuThreadState.Memory.ReadSafe<ulong>(
                        (uint) (this.CpuThreadState.GPR[29] + ((MaxGprIndex - Index) * 4)));
                }
                throw(new NotImplementedException("Invalid operation"));
            }
        }

        class AstRegisterReader : RegisterReaderBase<AstNodeExpr>
        {
            public MethodInfo MethodInfo;
            public PspMemory PspMemory;

            public AstRegisterReader(MethodInfo MethodInfo, PspMemory PspMemory)
            {
                this.MethodInfo = MethodInfo;
                this.PspMemory = PspMemory;
            }


            protected override AstNodeExpr ReadFromFpr(Type Type, int Index)
            {
                return ast.FPR(Index);
            }

            protected override AstNodeExpr ReadFromGpr(Type Type, int Index)
            {
                return ast.GPR(Type, Index);
            }

            protected override AstNodeExpr ReadFromStack(Type Type, int Index)
            {
                return ast.MemoryGetValue(Type, PspMemory, ast.GPR_u(29) + ((MaxGprIndex - Index) * 4));
            }
        }

        abstract class RegisterReaderBase<TReturn>
        {
            public const int MaxGprIndex = 12;
            public int GprIndex = 4;
            public int FprIndex = 0;
            public List<ParamInfo> ParamInfoList = new List<ParamInfo>();

            public TReturn Read(ParameterInfo ParameterInfo)
            {
                return Read(ParameterInfo.ParameterType, ParameterInfo);
            }

            public TReturn Read<TType>(ParameterInfo ParameterInfo)
            {
                return Read(typeof(TType), ParameterInfo);
            }

            abstract protected TReturn ReadFromFpr(Type Type, int Index);
            abstract protected TReturn ReadFromGpr(Type Type, int Index);
            abstract protected TReturn ReadFromStack(Type Type, int Index);

            public TReturn Read(Type Type, ParameterInfo ParameterInfo)
            {
                var IsFloat = (Type == typeof(float));
                var IsInt32 = (Type == typeof(uint) || Type == typeof(int));
                var IsInt64 = (Type == typeof(ulong) || Type == typeof(long));
                int SizeInWords = Marshal.SizeOf(Type) / 4;
                if (IsFloat)
                {
                    try
                    {
                        return ReadFromFpr(Type, FprIndex);
                    }
                    finally
                    {
                        ParamInfoList.Add(new ParamInfo()
                        {
                            ParameterInfo = ParameterInfo,
                            RegisterType = ParamInfo.RegisterTypeEnum.Fpr,
                            RegisterIndex = FprIndex,
                        });

                        FprIndex++;
                    }
                }
                else if (IsInt32 || IsInt64)
                {
                    try
                    {
                        while (GprIndex % SizeInWords != 0) GprIndex++;

                        if (GprIndex >= MaxGprIndex)
                        {
                            //Console.WriteLine("{0}: STACK[{1}]", MethodInfo.Name, ((MaxGprIndex - GprIndex) * 4));
                            return ReadFromStack(Type, GprIndex);
                        }
                        else
                        {
                            //Console.WriteLine("{0}: {1}", MethodInfo.Name, GprIndex);
                            return ReadFromGpr(Type, GprIndex);
                        }
                    }
                    finally
                    {
                        ParamInfoList.Add(new ParamInfo()
                        {
                            ParameterInfo = ParameterInfo,
                            RegisterType = ParamInfo.RegisterTypeEnum.Gpr,
                            RegisterIndex = GprIndex,
                        });

                        GprIndex += SizeInWords;
                    }
                }
                else
                {
                    throw(new NotImplementedException("Can't handle type " + Type));
                }
            }
        }

        private AstNodeStmContainer CreateDelegateForMethodInfoPriv(MethodInfo MethodInfo,
            HlePspFunctionAttribute HlePspFunctionAttribute, out List<ParamInfo> OutParamInfoList)
        {
            var RegisterReader = new AstRegisterReader(MethodInfo, Memory);
            OutParamInfoList = RegisterReader.ParamInfoList;

            //var SafeILGenerator = MipsMethodEmiter.SafeILGenerator;

            var AstNodes = new AstNodeStmContainer();

            AstNodes.AddStatement(ast.Comment("HleModuleHost.CreateDelegateForMethodInfo(" + MethodInfo + ", " +
                                              HlePspFunctionAttribute + ")"));

            AstNodeExprCall AstMethodCall;
            {
                //var ModuleObject = this.Cast(this.GetType(), this.FieldAccess(this.Argument<CpuThreadState>(0, "CpuThreadState"), "ModuleObject"));
                //SafeILGenerator.LoadArgument0CpuThreadState();
                //SafeILGenerator.LoadField(typeof(CpuThreadState).GetField("ModuleObject"));
                //SafeILGenerator.CastClass(this.GetType());

                var AstParameters = new List<AstNodeExpr>();

                foreach (var ParameterInfo in MethodInfo.GetParameters())
                {
                    var ParameterType = ParameterInfo.ParameterType;

                    var HleInvalidAsNullAttribute = ParameterInfo.GetCustomAttribute<HleInvalidAsNullAttribute>();
                    var HleInvalidAsInvalidPointerAttribute =
                        ParameterInfo.GetCustomAttribute<HleInvalidAsInvalidPointerAttribute>();
                    InvalidAddressAsEnum InvalidAddressAsEnum = InvalidAddressAsEnum.Exception;
                    if (HleInvalidAsNullAttribute != null) InvalidAddressAsEnum = InvalidAddressAsEnum.Null;
                    if (HleInvalidAsInvalidPointerAttribute != null)
                        InvalidAddressAsEnum = InvalidAddressAsEnum.InvalidAddress;

                    // The CpuThreadState
                    if (ParameterType == typeof(CpuThreadState))
                    {
                        AstParameters.Add(ast.CpuThreadState);
                    }
                    // A stringz
                    else if (ParameterType == typeof(string))
                    {
                        AstParameters.Add(
                            ast.CallStatic(
                                (Func<CpuThreadState, uint, string>) HleModuleHost.StringFromAddress,
                                ast.CpuThreadState,
                                RegisterReader.Read<uint>(ParameterInfo)
                            )
                        );
                    }
                    // A pointer or ref/out
                    else if (ParameterType.IsPointer || ParameterType.IsByRef)
                    {
                        AstParameters.Add(
                            ast.Cast(
                                ParameterType,
                                ast.MemoryGetPointer(
                                    CpuProcessor.Memory,
                                    RegisterReader.Read<uint>(ParameterInfo),
                                    Safe: true,
                                    ErrorDescription: "Invalid Pointer for Argument '" + ParameterType.Name + " " +
                                                      ParameterInfo.Name + "'",
                                    InvalidAddress: InvalidAddressAsEnum
                                )
                            )
                        );
                    }
                    // A long type
                    else if (ParameterType == typeof(long) || ParameterType == typeof(ulong))
                    {
                        AstParameters.Add(RegisterReader.Read(ParameterInfo));
                    }
                    // A float register.
                    else if (ParameterType == typeof(float))
                    {
                        AstParameters.Add(RegisterReader.Read(ParameterInfo));
                    }
                    // PspPointer
                    else if (ParameterType == typeof(PspPointer))
                    {
                        AstParameters.Add(ast.CallStatic(
                            typeof(PspPointer).GetMethod("op_Implicit", new[] {typeof(uint)}),
                            RegisterReader.Read<uint>(ParameterInfo)
                        ));
                    }
                    // A class
                    else if (ParameterType.IsClass)
                    {
                        if (!ParameterType.Implements(typeof(IHleUidPoolClass)))
                        {
                            throw (new InvalidCastException(String.Format(
                                "Can't use a class '{0}' not implementing IHleUidPoolClass as parameter",
                                ParameterType)));
                        }

                        AstParameters.Add(ast.Cast(ParameterType, ast.CallStatic(
                            (Func<CpuThreadState, Type, int, bool, object>) GetObjectFromPoolHelper,
                            ast.CpuThreadState,
                            ast.Immediate(ParameterType),
                            RegisterReader.Read<int>(ParameterInfo),
                            (InvalidAddressAsEnum == InvalidAddressAsEnum.Null)
                        )));
                    }
                    // An integer register
                    else
                    {
                        AstParameters.Add(ast.Cast(ParameterType,
                            RegisterReader.Read((ParameterType == typeof(uint)) ? typeof(uint) : typeof(int),
                                ParameterInfo)));
                    }
                }

                AstMethodCall = ast.CallInstance(
                    ThisILInstanceHolder.GetAstFieldAccess(),
                    MethodInfo,
                    AstParameters.ToArray()
                );
            }

            if (AstMethodCall.Type == typeof(void)) AstNodes.AddStatement(ast.Statement(AstMethodCall));
            else if (AstMethodCall.Type == typeof(long))
                AstNodes.AddStatement(ast.Assign(ast.GPR_l(2), ast.Cast<long>(AstMethodCall)));
            else if (AstMethodCall.Type == typeof(float))
                AstNodes.AddStatement(ast.Assign(ast.FPR(0), ast.Cast<float>(AstMethodCall)));
            else if (AstMethodCall.Type.IsClass)
            {
                if (!AstMethodCall.Type.Implements(typeof(IHleUidPoolClass)))
                {
                    throw (new InvalidCastException(String.Format(
                        "Can't use a class '{0}' not implementing IHleUidPoolClass as return value",
                        AstMethodCall.Type)));
                }
                AstNodes.AddStatement(ast.Assign(
                    ast.GPR(2),
                    ast.CallStatic(
                        (Func<CpuThreadState, Type, IHleUidPoolClass, uint>) GetOrAllocIndexFromPoolHelper,
                        ast.CpuThreadState,
                        ast.Immediate(AstMethodCall.Type),
                        ast.Cast<IHleUidPoolClass>(AstMethodCall)
                    )
                ));
            }
            else AstNodes.AddStatement(ast.Assign(ast.GPR(2), ast.Cast<uint>(AstMethodCall)));

            return AstNodes;
        }

        private Action<CpuThreadState> CreateDelegateForMethodInfo(MethodInfo MethodInfo,
            HlePspFunctionAttribute HlePspFunctionAttribute)
        {
            if (!MethodInfo.DeclaringType.IsAssignableFrom(this.GetType()))
            {
                throw (new Exception(String.Format("Invalid {0} != {1}", MethodInfo.DeclaringType, this.GetType())));
            }

            bool SkipLog = HlePspFunctionAttribute.SkipLog;
            var NotImplementedAttribute = (HlePspNotImplementedAttribute) MethodInfo
                .GetCustomAttributes(typeof(HlePspNotImplementedAttribute), true).FirstOrDefault();
            bool NotImplementedFunc = (NotImplementedAttribute != null) && NotImplementedAttribute.Notice;

            List<ParamInfo> ParamInfoList;
            var AstNodes = AstOptimizerPsp.GlobalOptimize(
                CpuProcessor,
                ast.Statements(
                    // Do stuff before
                    CreateDelegateForMethodInfoPriv(MethodInfo, HlePspFunctionAttribute, out ParamInfoList)
                    // Do stuff after
                )
            );

            var Delegate = AstNodeExtensions._GeneratorILPsp.GenerateDelegate<Action<CpuThreadState>>(
                String.Format("Proxy_{0}_{1}", this.GetType().Name, MethodInfo.Name),
                AstNodes
            );

            //return Delegate;

            return (CpuThreadState) =>
            {
                bool Trace = (!SkipLog && CpuThreadState.CpuProcessor.CpuConfig.DebugSyscalls);
                bool NotImplemented = NotImplementedFunc && HleConfig.DebugNotImplemented;

                if (Trace && (MethodInfo.DeclaringType.Name == "Kernel_Library")) Trace = false;

                //Console.WriteLine("aaaaaaaaaaaaa");

                if (NotImplemented)
                {
                    Trace = true;
                    ConsoleUtils.SaveRestoreConsoleState(() =>
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(
                            "Not implemented {0}.{1}",
                            MethodInfo.DeclaringType.Name, MethodInfo.Name
                        );
                    });
                }

                var Out = Console.Out;
                if (NotImplemented)
                {
                    Out = Console.Error;
                }

                if (Trace)
                {
                    if (ThreadManager.Current != null)
                    {
                        Out.Write(
                            "Thread({0}:'{1}') : RA(0x{2:X})",
                            ThreadManager.Current.Id,
                            ThreadManager.Current.Name,
                            ThreadManager.Current.CpuThreadState.RA
                        );
                    }
                    else
                    {
                        Out.Write("NoThread:");
                    }
                    Out.Write(" : {0}.{1}", MethodInfo.DeclaringType.Name, MethodInfo.Name);
                    Out.Write("(");
                    int Count = 0;

                    var NormalRegisterReader = new NormalRegisterReader(CpuThreadState);
                    foreach (var ParamInfo in ParamInfoList)
                    {
                        if (Count > 0) Out.Write(", ");
                        Out.Write("{0}:", ParamInfo.ParameterInfo.Name);
                        switch (ParamInfo.RegisterType)
                        {
                            case HleModuleHost.ParamInfo.RegisterTypeEnum.Fpr:
                            case HleModuleHost.ParamInfo.RegisterTypeEnum.Gpr:
                                var Object = NormalRegisterReader.Read<uint>(ParamInfo.ParameterInfo);
                                Out.Write("{0}",
                                    ToNormalizedTypeString(ParamInfo.ParameterInfo.ParameterType, CpuThreadState,
                                        Object));
                                break;
                            default:
                                throw (new NotImplementedException());
                        }
                        Count++;
                    }
                    Out.Write(")");
                    //Console.WriteLine("");
                }

                try
                {
                    CpuThreadState.PC = CpuThreadState.RA;
                    Delegate(CpuThreadState);
                }
                catch (InvalidProgramException)
                {
                    Console.WriteLine("CALLING: {0}", MethodInfo);
                    Console.WriteLine("{0}", (new GeneratorCSharp()).GenerateRoot(AstNodes).ToString());

                    foreach (var Line in AstNodeExtensions._GeneratorILPsp.GenerateToStringList(MethodInfo, AstNodes))
                    {
                        Console.WriteLine(Line);
                    }

                    throw;
                }
                catch (MemoryPartitionNoMemoryException)
                {
                    CpuThreadState.GPR[2] = (int) SceKernelErrors.ERROR_ERRNO_NO_MEMORY;
                }
                catch (SceKernelException SceKernelException)
                {
                    CpuThreadState.GPR[2] = (int) SceKernelException.SceKernelError;
                }
                catch (SceKernelSelfStopUnloadModuleException)
                {
                    throw;
                }
#if !DO_NOT_PROPAGATE_EXCEPTIONS
                catch (Exception Exception)
                {
                    throw (new Exception(
                        String.Format("ERROR calling {0}.{1}!", MethodInfo.DeclaringType.Name, MethodInfo.Name),
                        Exception
                    ));
                }
#endif
                finally
                {
                    if (Trace)
                    {
                        Out.WriteLine(" : {0}",
                            ToNormalizedTypeString(MethodInfo.ReturnType, CpuThreadState,
                                ((MethodInfo.ReturnType == typeof(float))
                                    ? (object) CpuThreadState.FPR[0]
                                    : (object) CpuThreadState.GPR[2])));
                        Out.WriteLine("");
                    }
                }
            };
        }

        public static string ToNormalizedTypeString(Type ParameterType, CpuThreadState CpuThreadState, object Value)
        {
            if (ParameterType == typeof(void))
            {
                return "void";
            }

            if (ParameterType == typeof(string))
            {
                return String.Format("'{0}'", StringFromAddress(CpuThreadState, (uint) Convert.ToInt64(Value)));
            }

            if (ParameterType == typeof(int))
            {
                return String.Format("{0}", Convert.ToInt32(Value));
            }

            if (ParameterType.IsEnum)
            {
                var Name = ParameterType.GetEnumName(Value);
                if (string.IsNullOrEmpty(Name)) Name = Value.ToString();
                return Name;
            }

            if (ParameterType.IsPointer)
            {
                return String.Format("0x{0:X8}",
                    CpuThreadState.CpuProcessor.Memory.PointerToPspAddressUnsafe((void*) Convert.ToInt64(Value)));
            }

            if (ParameterType == typeof(float))
            {
                return String.Format("{0}", Convert.ToSingle(Value));
            }

            return String.Format("0x{0:X8}", Value);
        }
    }
}