using CSharpUtils;
using CSharpUtils.Threading;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Optimizers;
using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.InstructionCache
{
    public sealed class MethodCache : IInjectInitialize
    {
        public static readonly MethodCache Methods = new MethodCache();

        private static readonly AstMipsGenerator Ast = AstMipsGenerator.Instance;
        private static readonly GeneratorIL GeneratorIlInstance = new GeneratorIL();

        private readonly Dictionary<uint, MethodCacheInfo> _methodMapping =
            new Dictionary<uint, MethodCacheInfo>(64 * 1024);

        public IEnumerable<uint> PCs => _methodMapping.Keys;

        [Inject] public CpuProcessor CpuProcessor;

        MethodCompilerThread _methodCompilerThread;

        void IInjectInitialize.Initialize() => _methodCompilerThread = new MethodCompilerThread(CpuProcessor, this);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
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

        internal void Free(MethodCacheInfo methodCacheInfo) => _methodMapping.Remove(methodCacheInfo.EntryPC);

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
                if (methodCacheInfo.MaxPC >= start && methodCacheInfo.MinPC <= end)
                {
                    methodCacheInfo.Free();
                }
            }
        }

        public void _MethodCacheInfo_SetInternal(CpuThreadState cpuThreadState, MethodCacheInfo methodCacheInfo,
            uint pc)
        {
            methodCacheInfo.SetDynarecFunction(_methodCompilerThread.GetDynarecFunctionForPC(pc));
        }
    }

    internal class MethodCompilerThread
    {
        private Thread Thread;
        private Dictionary<uint, DynarecFunction> Functions = new Dictionary<uint, DynarecFunction>();
        private HashSet<uint> ExploringPCs = new HashSet<uint>();
        private ThreadMessageBus<uint> ExploreQueue = new ThreadMessageBus<uint>();
        private CpuProcessor CpuProcessor;
        private MethodCache MethodCache;

        public MethodCompilerThread(CpuProcessor cpuProcessor, MethodCache methodCache)
        {
            CpuProcessor = cpuProcessor;
            MethodCache = methodCache;
            Thread = new Thread(Main)
            {
                IsBackground = true
            };
            Thread.Start();
        }

        private bool _ShouldAdd(uint pc) => !Functions.ContainsKey(pc) && !ExploringPCs.Contains(pc);

        private void _AddedPC(uint pc) => ExploringPCs.Add(pc);

        public void AddPcNow(uint pc)
        {
            lock (this)
            {
                if (!_ShouldAdd(pc)) return;
                _AddedPC(pc);
                ExploreQueue.AddFirst(pc);
            }
        }

        public void AddPcLater(uint pc)
        {
            lock (this)
            {
                if (!_ShouldAdd(pc)) return;
                _AddedPC(pc);
                ExploreQueue.AddLast(pc);
            }
        }

        AutoResetEvent _completedFunction = new AutoResetEvent(false);

        private void Main()
        {
            Console.WriteLine("MethodCache.Start()");
            try
            {
                while (true)
                {
                    var pc = ExploreQueue.ReadOne();
                    //Console.Write("Compiling {0:X8}...", PC);
                    var dynarecFunction = _GenerateForPC(pc);
                    lock (this) Functions[pc] = dynarecFunction;
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
            var memory = CpuProcessor.Memory;
            if (_DynarecConfig.DebugFunctionCreation)
            {
                Console.Write("PC=0x{0:X8}...", pc);
            }
            //var Stopwatch = new Logger.Stopwatch();
            var Time0 = DateTime.UtcNow;

            var DynarecFunction =
                CpuProcessor.DynarecFunctionCompiler.CreateFunction(
                    new InstructionStreamReader(new PspMemoryStream(memory)), pc);
            if (DynarecFunction.EntryPc != pc) throw (new Exception("Unexpected error"));

            if (_DynarecConfig.AllowCreatingUsedFunctionsInBackground)
            {
                foreach (var CallingPC in DynarecFunction.CallingPCs)
                {
                    if (PspMemory.IsAddressValid(CallingPC))
                    {
                        AddPcLater(CallingPC);
                    }
                }
            }

            var Time1 = DateTime.UtcNow;

            if (_DynarecConfig.ImmediateLinking)
            {
                try
                {
                    if (Platform.IsMono) Marshal.Prelink(DynarecFunction.Delegate.Method);
                    DynarecFunction.Delegate(null);
                }
                catch (InvalidProgramException)
                {
                    Console.Error.WriteLine("Invalid delegate:");
                    Console.Error.WriteLine(DynarecFunction.AstNode.ToCSharpString());
                    Console.Error.WriteLine(DynarecFunction.AstNode.ToIlString<Action<CpuThreadState>>());
                    throw;
                }
            }

            var Time2 = DateTime.UtcNow;

            DynarecFunction.TimeLinking = Time2 - Time1;
            var TimeAstGeneration = Time1 - Time0;

            if (_DynarecConfig.DebugFunctionCreation)
            {
                ConsoleUtils.SaveRestoreConsoleColor(
                    ((TimeAstGeneration + DynarecFunction.TimeLinking).TotalMilliseconds > 10)
                        ? ConsoleColor.Red
                        : ConsoleColor.Gray, () =>
                    {
                        Console.WriteLine(
                            "({0}): (analyze: {1}, generateAST: {2}, optimize: {3}, generateIL: {4}, createDelegate: {5}, link: {6}): ({1}, {2}, {3}, {4}, {5}, {6}) : {7} ms",
                            (DynarecFunction.MaxPc - DynarecFunction.MinPc) / 4,
                            (int) DynarecFunction.TimeAnalyzeBranches.TotalMilliseconds,
                            (int) DynarecFunction.TimeGenerateAst.TotalMilliseconds,
                            (int) DynarecFunction.TimeOptimize.TotalMilliseconds,
                            (int) DynarecFunction.TimeGenerateIl.TotalMilliseconds,
                            (int) DynarecFunction.TimeCreateDelegate.TotalMilliseconds,
                            (int) DynarecFunction.TimeLinking.TotalMilliseconds,
                            (int) (TimeAstGeneration + DynarecFunction.TimeLinking).TotalMilliseconds
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

            return DynarecFunction;
        }

        public DynarecFunction GetDynarecFunctionForPC(uint PC)
        {
            lock (this)
            {
                if (!Functions.ContainsKey(PC))
                {
                    Functions[PC] =
                        CpuProcessor.DynarecFunctionCompiler.CreateFunction(
                            new InstructionStreamReader(new PspMemoryStream(CpuProcessor.Memory)), PC);
                }
                return Functions[PC];
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