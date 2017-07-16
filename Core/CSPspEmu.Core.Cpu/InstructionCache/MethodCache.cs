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

		private static readonly AstMipsGenerator ast = AstMipsGenerator.Instance;
		private static readonly GeneratorIL GeneratorILInstance = new GeneratorIL();

		private readonly Dictionary<uint, MethodCacheInfo> MethodMapping = new Dictionary<uint, MethodCacheInfo>(64 * 1024);
		public IEnumerable<uint> PCs { get { return MethodMapping.Keys; } }

		[Inject]
		public CpuProcessor CpuProcessor;

		MethodCompilerThread MethodCompilerThread;

		void IInjectInitialize.Initialize()
		{
			MethodCompilerThread = new MethodCompilerThread(CpuProcessor, this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PC"></param>
		/// <returns></returns>
		public MethodCacheInfo GetForPC(uint PC)
		{
			if (MethodMapping.ContainsKey(PC))
			{
				return MethodMapping[PC];
			}
			else
			{
				var DelegateGeneratorForPC = GeneratorILInstance.GenerateDelegate<Action<CpuThreadState>>("MethodCache.DynamicCreateNewFunction", ast.Statements(
					ast.Statement(ast.CallInstance(ast.CpuThreadState, (Action<MethodCacheInfo, uint>)CpuThreadState.Methods._MethodCacheInfo_SetInternal, ast.GetMethodCacheInfoAtPC(PC), PC)),
					ast.Statement(ast.TailCall(ast.CallInstance(ast.GetMethodCacheInfoAtPC(PC), (Action<CpuThreadState>)MethodCacheInfo.Methods.CallDelegate, ast.CpuThreadState))),
					ast.Return()
				));
				return MethodMapping[PC] = new MethodCacheInfo(this, DelegateGeneratorForPC, PC);
			}
		}

		internal void Free(MethodCacheInfo MethodCacheInfo)
		{
			MethodMapping.Remove(MethodCacheInfo.EntryPC);
		}

		public void FlushAll()
		{
			foreach (var MethodCacheInfo in MethodMapping.Values.ToArray())
			{
				MethodCacheInfo.Free();
			}
		}

		public void FlushRange(uint Start, uint End)
		{
			//Console.Error.WriteLine("[{0}]", MethodMapping);
			//Console.Error.WriteLine("[{0}]", MethodMapping.Values);
			foreach (var MethodCacheInfo in MethodMapping.Values.ToArray())
			//foreach (var MethodCacheInfo in MethodMapping.Values)
			{
				//Console.Error.WriteLine("[{0}]", MethodCacheInfo);
				if (MethodCacheInfo.MaxPC >= Start && MethodCacheInfo.MinPC <= End)
				{
					MethodCacheInfo.Free();
				}
			}
		}

		public void _MethodCacheInfo_SetInternal(CpuThreadState CpuThreadState, MethodCacheInfo MethodCacheInfo, uint PC)
		{
			MethodCacheInfo.SetDynarecFunction(MethodCompilerThread.GetDynarecFunctionForPC(PC));
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
			this.CpuProcessor = cpuProcessor;
			this.MethodCache = methodCache;
			this.Thread = new Thread(Main)
			{
				Name = "MethodCache.MethodCompilerThread",
				IsBackground = true,
			};
			this.Thread.Start();
		}

		private bool _ShouldAdd(uint pc)
		{
			return !Functions.ContainsKey(pc) && !ExploringPCs.Contains(pc);
		}

		private void _AddedPC(uint pc)
		{
			//Console.WriteLine("Enqueing: {0:X8}", PC);
			ExploringPCs.Add(pc);
		}

		public void AddPCNow(uint pc)
		{
			lock (this)
			{
				if (_ShouldAdd(pc))
				{
					_AddedPC(pc);
					ExploreQueue.AddFirst(pc);
				}
			}
		}

		public void AddPCLater(uint pc)
		{
			lock (this)
			{
				if (_ShouldAdd(pc))
				{
					_AddedPC(pc);
					ExploreQueue.AddLast(pc);
				}
			}
		}

		AutoResetEvent CompletedFunction = new AutoResetEvent(false);

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
					lock (this) this.Functions[pc] = dynarecFunction;
					//Console.WriteLine("Ok");
					CompletedFunction.Set();
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
			var time0 = DateTime.UtcNow;

			var dynarecFunction = CpuProcessor.DynarecFunctionCompiler.CreateFunction(new InstructionStreamReader(new PspMemoryStream(memory)), pc);
			if (dynarecFunction.EntryPC != pc) throw (new Exception("Unexpected error"));

			if (_DynarecConfig.AllowCreatingUsedFunctionsInBackground)
			{
				foreach (var callingPc in dynarecFunction.CallingPCs)
				{
					if (PspMemory.IsAddressValid(callingPc))
					{
						AddPCLater(callingPc);
					}
				}
			}

			var time1 = DateTime.UtcNow;

			if (_DynarecConfig.ImmediateLinking)
			{
				try
				{
					if (Platform.IsMono) Marshal.Prelink(dynarecFunction.Delegate.Method);
					dynarecFunction.Delegate(null);
				}
				catch (InvalidProgramException invalidProgramException)
				{
					Console.Error.WriteLine("Invalid delegate:");
					Console.Error.WriteLine(dynarecFunction.AstNode.ToCSharpString());
					Console.Error.WriteLine(dynarecFunction.AstNode.ToIlString<Action<CpuThreadState>>());
					throw;
				}
			}

			var time2 = DateTime.UtcNow;

			dynarecFunction.TimeLinking = time2 - time1;
			var TimeAstGeneration = time1 - time0;

			if (_DynarecConfig.DebugFunctionCreation)
			{
				ConsoleUtils.SaveRestoreConsoleColor(((TimeAstGeneration + dynarecFunction.TimeLinking).TotalMilliseconds > 10) ? ConsoleColor.Red : ConsoleColor.Gray, () =>
				{
					Console.WriteLine(
						"({0}): (analyze: {1}, generateAST: {2}, optimize: {3}, generateIL: {4}, createDelegate: {5}, link: {6}): ({1}, {2}, {3}, {4}, {5}, {6}) : {7} ms",
						(dynarecFunction.MaxPC - dynarecFunction.MinPC) / 4,
						(int)dynarecFunction.TimeAnalyzeBranches.TotalMilliseconds,
						(int)dynarecFunction.TimeGenerateAst.TotalMilliseconds,
						(int)dynarecFunction.TimeOptimize.TotalMilliseconds,
						(int)dynarecFunction.TimeGenerateIL.TotalMilliseconds,
						(int)dynarecFunction.TimeCreateDelegate.TotalMilliseconds,
						(int)dynarecFunction.TimeLinking.TotalMilliseconds,
						(int)(TimeAstGeneration + dynarecFunction.TimeLinking).TotalMilliseconds
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

		public DynarecFunction GetDynarecFunctionForPC(uint pc)
		{
			lock (this)
			{
				if (!Functions.ContainsKey(pc))
				{
					Functions[pc] = CpuProcessor.DynarecFunctionCompiler.CreateFunction(new InstructionStreamReader(new PspMemoryStream(CpuProcessor.Memory)), pc);
				}
				return Functions[pc];
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
