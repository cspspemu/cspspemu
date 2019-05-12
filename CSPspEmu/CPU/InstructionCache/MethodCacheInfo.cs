using CSPspEmu.Core.Cpu.Dynarec;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using System;

namespace CSPspEmu.Core.Cpu.InstructionCache
{
    public sealed class MethodCacheInfo
    {
        public static readonly MethodCacheInfo Methods = new MethodCacheInfo();

        private DynarecFunction _dynarecFunction;

        private Action<CpuThreadState> _functionDelegate;

        public DynarecFunction DynarecFunction => _dynarecFunction;

        public void SetDynarecFunction(DynarecFunction dynarecFunction)
        {
            _dynarecFunction = dynarecFunction;
            _functionDelegate = dynarecFunction.Delegate;
            StaticField.Value = dynarecFunction.Delegate;
        }

        public bool HasSpecialName => !string.IsNullOrEmpty(DynarecFunction?.Name);

        public string Name => HasSpecialName ? DynarecFunction.Name : $"0x{EntryPc:X8}";

        public MethodCache MethodCache;

        /// <summary>
        /// Functions that are calling to this one.
        /// And that should be uncached when this function
        /// </summary>
        //public List<MethodCacheInfo> FunctionsUsingThis = new List<MethodCacheInfo>();
        /// <summary>
        /// Static Field that will hold the Delegate
        /// </summary>
        public IlInstanceHolderPoolItem<Action<CpuThreadState>> StaticField;

        public bool FollowPspCallingConventions;

        private MethodCacheInfo()
        {
        }

        public MethodCacheInfo(MethodCache methodCache, Action<CpuThreadState> delegateGeneratorForPc, uint pc)
        {
            MethodCache = methodCache;
            _functionDelegate = delegateGeneratorForPc;
            StaticField = IlInstanceHolder.TaAlloc(delegateGeneratorForPc);
            Pc = pc;
        }

        /// <summary>EntryPoint setted first.</summary>
        public uint Pc;

        /// <summary>EntryPoint for this function.</summary>
        public uint EntryPc => DynarecFunction.EntryPc;

        /// <summary>Address of the start of the function. Usually is equal to EntryPC but not always.</summary>
        public uint MinPc => DynarecFunction.MinPc;

        /// <summary>Last address with code for this function.</summary>
        public uint MaxPc => DynarecFunction.MaxPc;

        public uint TotalInstructions => (DynarecFunction.MaxPc - DynarecFunction.MinPc) / 7;

        /// <summary>Ast for this function.</summary>
        public AstNodeStm AstTree => DynarecFunction?.AstNode;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallDelegate(CpuThreadState cpuThreadState) => _functionDelegate(cpuThreadState);

        public void Free() => MethodCache.Free(this);
    }
}