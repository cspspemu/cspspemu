using CSPspEmu.Core.Cpu.Emitter;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Optimizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.CodeCache
{
	public class MethodCache
	{
		public MethodCacheInfo[] Methods = new MethodCacheInfo[16 * 1024];
		private LinkedList<int> MethodsFree = new LinkedList<int>();
		private Dictionary<uint, MethodCacheInfo> MethodMapping = new Dictionary<uint, MethodCacheInfo>();

		public MethodCache()
		{
			for (int n = 0; n < Methods.Length; n++) MethodsFree.AddLast(n);
		}

		static private AstGenerator ast = AstGenerator.Instance;

		public MethodCacheInfo GetForPC(uint PC)
		{
			if (MethodMapping.ContainsKey(PC))
			{
				return MethodMapping[PC];
			}
			else
			{
				var FreeIndex = MethodsFree.RemoveFirstAndGet();

				var Ast = (AstNodeStm)new AstOptimizer().Optimize(ast.Statements(
					ast.Statement(ast.CallInstance(MipsMethodEmitter.CpuThreadStateArgument(), (Action<MethodCacheInfo, uint>)CpuThreadState.Methods._MethodCacheInfo_SetInternal, MipsMethodEmitter.MethodCacheInfoGetAtIndex(FreeIndex), PC)),
					ast.Statement(ast.CallTail(ast.CallDelegate(ast.FieldAccess(MipsMethodEmitter.MethodCacheInfoGetAtIndex(FreeIndex), "Delegate"), MipsMethodEmitter.CpuThreadStateArgument()))),
					ast.Return()
				));

				//Console.WriteLine(Ast.ToCSharpString());
				//Console.WriteLine(Ast.ToILString(typeof(Action<CpuThreadState>).GetMethod("Invoke")));

				var MethodCacheInfo = new MethodCacheInfo()
				{
					MethodIndex = FreeIndex,
					Delegate = GeneratorIL.GenerateDelegate<GeneratorIL, Action<CpuThreadState>>(
						"MethodCache.DynamicCreateNewFunction",
						Ast
					),
				};

				Methods[FreeIndex] = MethodCacheInfo;
				MethodMapping[PC] = MethodCacheInfo;

				return MethodCacheInfo;
			}
		}

		private void Remove(MethodCacheInfo MethodCacheInfo)
		{
			MethodsFree.AddLast(MethodCacheInfo.MethodIndex);
			MethodMapping.Remove(MethodCacheInfo.EntryPC);
			MethodCacheInfo.MethodIndex = -1;
		}

		public void FlushAll()
		{
			foreach (var MethodCacheInfo in MethodMapping.Values.ToArray())
			{
				Remove(MethodCacheInfo);
			}
		}

		public void FlushRange(uint Start, uint End)
		{
			foreach (var MethodCacheInfo in MethodMapping.Values.ToArray())
			{
				if (MethodCacheInfo.MaxPC >= Start && MethodCacheInfo.MinPC <= End)
				{
					Remove(MethodCacheInfo);
				}
			}
		}
	}
}
