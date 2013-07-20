using CSharpUtils;
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
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.InstructionCache
{
	public sealed class MethodCache
	{
		public static readonly MethodCache Methods = new MethodCache();

		private static readonly AstMipsGenerator ast = AstMipsGenerator.Instance;
		private static readonly GeneratorIL GeneratorILInstance = new GeneratorIL();

		private readonly Dictionary<uint, MethodCacheInfo> MethodMapping = new Dictionary<uint, MethodCacheInfo>(64 * 1024);
		public IEnumerable<uint> PCs { get { return MethodMapping.Keys; } }

		public MethodCache()
		{
		}

		private static Action<CpuThreadState> GetGeneratorForPC(uint PC)
		{
			var Ast = ast.Statements(
				ast.Statement(ast.CallInstance(ast.CpuThreadState, (Action<MethodCacheInfo, uint>)CpuThreadState.Methods._MethodCacheInfo_SetInternal, ast.GetMethodCacheInfoAtPC(PC), PC)),
				ast.Statement(ast.TailCall(ast.CallInstance(ast.GetMethodCacheInfoAtPC(PC), (Action<CpuThreadState>)MethodCacheInfo.Methods.CallDelegate, ast.CpuThreadState))),
				ast.Return()
			);

			return GeneratorILInstance.GenerateDelegate<Action<CpuThreadState>>("MethodCache.DynamicCreateNewFunction", Ast);
		}

		public MethodCacheInfo GetForPC(uint PC)
		{
			if (MethodMapping.ContainsKey(PC))
			{
				return MethodMapping[PC];
			}
			else
			{
				var DelegateGeneratorForPC = GetGeneratorForPC(PC);
				return MethodMapping[PC] = new MethodCacheInfo(this, DelegateGeneratorForPC);
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
			foreach (var MethodCacheInfo in MethodMapping.Values.ToArray())
			{
				if (MethodCacheInfo.MaxPC >= Start && MethodCacheInfo.MinPC <= End)
				{
					MethodCacheInfo.Free();
				}
			}
		}

		public void _MethodCacheInfo_SetInternal(CpuThreadState CpuThreadState, MethodCacheInfo MethodCacheInfo, uint PC)
		{
			var Memory = CpuThreadState.Memory;
			var CpuProcessor = CpuThreadState.CpuProcessor;
			if (_DynarecConfig.DebugFunctionCreation)
			{
				Console.Write("PC=0x{0:X8}...", PC);
			}
			//var Stopwatch = new Logger.Stopwatch();
			var Time0 = DateTime.UtcNow;

			var DynarecFunction = CpuProcessor.DynarecFunctionCompiler.CreateFunction(new InstructionStreamReader(new PspMemoryStream(Memory)), PC);
			if (DynarecFunction.EntryPC != PC) throw (new Exception("Unexpected error"));

			var Time1 = DateTime.UtcNow;

			//try
			//{
				DynarecFunction.Delegate(null);
			//}
			//catch (InvalidProgramException InvalidProgramException)
			//{
			//	//Console.Error.WriteLine(DynarecFunction.AstNode.ToXmlString());
			//	throw (InvalidProgramException);
			//}

			var Time2 = DateTime.UtcNow;

			DynarecFunction.TimeLinking = Time2 - Time1;
			var TimeAstGeneration = Time1 - Time0;

			if (_DynarecConfig.DebugFunctionCreation)
			{
				ConsoleUtils.SaveRestoreConsoleColor(((TimeAstGeneration + DynarecFunction.TimeLinking).TotalMilliseconds > 10) ? ConsoleColor.Red : ConsoleColor.Gray, () =>
				{
					Console.WriteLine(
						"({0}): (analyze: {1}, generateAST: {2}, optimize: {3}, generateIL: {4}, createDelegate: {5}, link: {6}): ({1}, {2}, {3}, {4}, {5}, {6}) : {7} ms",
						(DynarecFunction.MaxPC - DynarecFunction.MinPC) / 4,
						(int)DynarecFunction.TimeAnalyzeBranches.TotalMilliseconds,
						(int)DynarecFunction.TimeGenerateAst.TotalMilliseconds,
						(int)DynarecFunction.TimeOptimize.TotalMilliseconds,
						(int)DynarecFunction.TimeGenerateIL.TotalMilliseconds,
						(int)DynarecFunction.TimeCreateDelegate.TotalMilliseconds,
						(int)DynarecFunction.TimeLinking.TotalMilliseconds,
						(int)(TimeAstGeneration + DynarecFunction.TimeLinking).TotalMilliseconds
					);
				});
			}

			//DynarecFunction.AstNode = DynarecFunction.AstNode.Optimize(CpuProcessor);

#if DEBUG_FUNCTION_CREATION
			CpuProcessor.DebugFunctionCreation = true;
#endif

			if (CpuProcessor.DebugFunctionCreation)
			{
				Console.WriteLine("-------------------------------------");
				Console.WriteLine("Created function for PC=0x{0:X8}", PC);
				Console.WriteLine("-------------------------------------");
				CpuThreadState.DumpRegistersCpu(Console.Out);
				Console.WriteLine("-------------------------------------");
				Console.WriteLine(DynarecFunction.AstNode.ToCSharpString());
				Console.WriteLine("-------------------------------------");
			}

			MethodCacheInfo.SetDynarecFunction(DynarecFunction);
		}
	}
}
