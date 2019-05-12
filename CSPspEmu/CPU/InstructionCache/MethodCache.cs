using CSharpUtils;
using CSharpUtils.Threading;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSPspEmu.Core.Cpu.InstructionCache
{
    public sealed class MethodCache : IInjectInitialize
    {
        public static readonly MethodCache Methods = new MethodCache();

        private static readonly AstMipsGenerator Ast = AstMipsGenerator.Instance;
        private static readonly GeneratorIl GeneratorIlInstance = new GeneratorIl();

        private readonly Dictionary<uint, MethodCacheInfo> _methodMapping =
            new Dictionary<uint, MethodCacheInfo>(64 * 1024);

        public IEnumerable<uint> PCs => _methodMapping.Keys;

        [Inject] public CpuProcessor CpuProcessor;

        MethodCompilerThread _methodCompilerThread;

        void IInjectInitialize.Initialize() => _methodCompilerThread = new MethodCompilerThread(CpuProcessor, this);
        
        public MethodCacheInfo GetForPc(uint pc)
        {
            if (_methodMapping.ContainsKey(pc))
            {
                return _methodMapping[pc];
            }
            else
            {
                var delegateGeneratorForPc = GeneratorIlInstance.GenerateDelegate<Action<CpuThreadState>>(
                    "MethodCache.DynamicCreateNewFunction", Ast.Statements(
                        Ast.Statement(Ast.CallInstance(Ast.CpuThreadStateExpr,
                            (Action<MethodCacheInfo, uint>) CpuThreadState.Methods._MethodCacheInfo_SetInternal,
                            Ast.GetMethodCacheInfoAtPc(pc), pc)),
                        Ast.Statement(Ast.TailCall(Ast.CallInstance(Ast.GetMethodCacheInfoAtPc(pc),
                            (Action<CpuThreadState>) MethodCacheInfo.Methods.CallDelegate, Ast.CpuThreadStateExpr))),
                        Ast.Return()
                    ));
                return _methodMapping[pc] = new MethodCacheInfo(this, delegateGeneratorForPc, pc);
            }
        }

        internal void Free(MethodCacheInfo methodCacheInfo) => _methodMapping.Remove(methodCacheInfo.EntryPc);

        public void FlushAll()
        {
            foreach (var methodCacheInfo in _methodMapping.Values.ToArray())
                methodCacheInfo.Free();
        }

        public void FlushRange(uint start, uint end)
        {
            //Console.Error.WriteLine("[{0}]", MethodMapping);
            //Console.Error.WriteLine("[{0}]", MethodMapping.Values);
            foreach (var methodCacheInfo in _methodMapping.Values.ToArray())
                //foreach (var MethodCacheInfo in MethodMapping.Values)
            {
                //Console.Error.WriteLine("[{0}]", MethodCacheInfo);
                if (methodCacheInfo.MaxPc >= start && methodCacheInfo.MinPc <= end)
                {
                    methodCacheInfo.Free();
                }
            }
        }

        public void _MethodCacheInfo_SetInternal(CpuThreadState cpuThreadState, MethodCacheInfo methodCacheInfo,
            uint pc)
        {
            methodCacheInfo.SetDynarecFunction(_methodCompilerThread.GetDynarecFunctionForPc(pc));
        }
    }

    internal class MethodCompilerThread
    {
        private readonly Dictionary<uint, DynarecFunction> _functions = new Dictionary<uint, DynarecFunction>();
        private readonly HashSet<uint> _exploringPCs = new HashSet<uint>();
        private readonly ThreadMessageBus<uint> _exploreQueue = new ThreadMessageBus<uint>();
        private readonly CpuProcessor _cpuProcessor;
        private MethodCache _methodCache;

        public MethodCompilerThread(CpuProcessor cpuProcessor, MethodCache methodCache)
        {
            _cpuProcessor = cpuProcessor;
            _methodCache = methodCache;
            var thread = new Thread(Main)
            {
                Name = "MethodCompilerThread",
                IsBackground = true
            };
            thread.Start();
        }

        private bool _ShouldAdd(uint pc) => !_functions.ContainsKey(pc) && !_exploringPCs.Contains(pc);

        private void _AddedPC(uint pc) => _exploringPCs.Add(pc);

        public void AddPcNow(uint pc)
        {
            lock (this)
            {
                if (!_ShouldAdd(pc)) return;
                _AddedPC(pc);
                _exploreQueue.AddFirst(pc);
            }
        }

        public void AddPcLater(uint pc)
        {
            lock (this)
            {
                if (!_ShouldAdd(pc)) return;
                _AddedPC(pc);
                _exploreQueue.AddLast(pc);
            }
        }

        private readonly AutoResetEvent _completedFunction = new AutoResetEvent(false);

        private void Main()
        {
            Console.WriteLine("MethodCache.Start()");
            try
            {
                while (true)
                {
                    var pc = _exploreQueue.ReadOne();
                    //Console.Write("Compiling {0:X8}...", PC);
                    var dynarecFunction = _GenerateForPC(pc);
                    lock (this) _functions[pc] = dynarecFunction;
                    //Console.WriteLine("Ok");
                    _completedFunction.Set();
                }
            }
            finally
            {
                Console.WriteLine("MethodCache.End()");
            }
        }

        private DynarecFunction _GenerateForPC(uint pc)
        {
            var memory = _cpuProcessor.Memory;
            if (DynarecConfig.DebugFunctionCreation)
            {
                Console.Write("PC=0x{0:X8}...", pc);
            }
            //var Stopwatch = new Logger.Stopwatch();
            var time0 = DateTime.UtcNow;

            var dynarecFunction =
                _cpuProcessor.DynarecFunctionCompiler.CreateFunction(
                    new InstructionStreamReader(new PspMemoryStream(memory)), pc);
            if (dynarecFunction.EntryPc != pc) throw (new Exception("Unexpected error"));

            if (DynarecConfig.AllowCreatingUsedFunctionsInBackground)
            {
                foreach (var callingPc in dynarecFunction.CallingPCs)
                {
                    if (PspMemory.IsAddressValid(callingPc))
                    {
                        AddPcLater(callingPc);
                    }
                }
            }

            var time1 = DateTime.UtcNow;

            if (DynarecConfig.ImmediateLinking)
            {
                try
                {
                    if (Platform.IsMono) Marshal.Prelink(dynarecFunction.Delegate.Method);
                    dynarecFunction.Delegate(null);
                }
                catch (InvalidProgramException)
                {
                    Console.Error.WriteLine("Invalid delegate:");
                    Console.Error.WriteLine(dynarecFunction.AstNode.ToCSharpString());
                    Console.Error.WriteLine(dynarecFunction.AstNode.ToIlString<Action<CpuThreadState>>());
                    throw;
                }
            }

            var time2 = DateTime.UtcNow;

            dynarecFunction.TimeLinking = time2 - time1;
            var timeAstGeneration = time1 - time0;

            if (DynarecConfig.DebugFunctionCreation)
            {
                ConsoleUtils.SaveRestoreConsoleColor(
                    (timeAstGeneration + dynarecFunction.TimeLinking).TotalMilliseconds > 10
                        ? ConsoleColor.Red
                        : ConsoleColor.Gray, () =>
                    {
                        Console.WriteLine(
                            "({0}): (analyze: {1}, generateAST: {2}, optimize: {3}, generateIL: {4}, createDelegate: {5}, link: {6}): ({1}, {2}, {3}, {4}, {5}, {6}) : {7} ms",
                            (dynarecFunction.MaxPc - dynarecFunction.MinPc) / 4,
                            (int) dynarecFunction.TimeAnalyzeBranches.TotalMilliseconds,
                            (int) dynarecFunction.TimeGenerateAst.TotalMilliseconds,
                            (int) dynarecFunction.TimeOptimize.TotalMilliseconds,
                            (int) dynarecFunction.TimeGenerateIl.TotalMilliseconds,
                            (int) dynarecFunction.TimeCreateDelegate.TotalMilliseconds,
                            (int) dynarecFunction.TimeLinking.TotalMilliseconds,
                            (int) (timeAstGeneration + dynarecFunction.TimeLinking).TotalMilliseconds
                        );
                    });
            }

            //DynarecFunction.AstNode = DynarecFunction.AstNode.Optimize(CpuProcessor);

#if DEBUG_FUNCTION_CREATION
					CpuProcessor.DebugFunctionCreation = true;
#endif

            //if (CpuProcessor.DebugFunctionCreation)
            //{
            //	Console.WriteLine("-------------------------------------");
            //	Console.WriteLine("Created function for PC=0x{0:X8}", PC);
            //	Console.WriteLine("-------------------------------------");
            //	CpuThreadState.DumpRegistersCpu(Console.Out);
            //	Console.WriteLine("-------------------------------------");
            //	Console.WriteLine(DynarecFunction.AstNode.ToCSharpString());
            //	Console.WriteLine("-------------------------------------");
            //}

            return dynarecFunction;
        }

        public DynarecFunction GetDynarecFunctionForPc(uint pc)
        {
            lock (this)
            {
                if (!_functions.ContainsKey(pc))
                {
                    _functions[pc] =
                        _cpuProcessor.DynarecFunctionCompiler.CreateFunction(
                            new InstructionStreamReader(new PspMemoryStream(_cpuProcessor.Memory)), pc);
                }
                return _functions[pc];
            }
        }

        //public DynarecFunction GetDynarecFunctionForPC(uint PC)
        //{
        //	//var DynarecFunction = CpuProcessor.DynarecFunctionCompiler.CreateFunction(new InstructionStreamReader(new PspMemoryStream(Memory)), PC);
        //
        //	//Console.WriteLine("+++++++++++++++++++++++++++++: {0:X8}", PC);
        //	while (true)
        //	{
        //		lock (this)
        //		{
        //			if (this.Functions.ContainsKey(PC))
        //			{
        //				//Console.WriteLine("-----------------------------");
        //				return this.Functions[PC];
        //			}
        //		}
        //		CompletedFunction.WaitOne();
        //		//Console.WriteLine("*****************************");
        //	}
        //}
    }
}