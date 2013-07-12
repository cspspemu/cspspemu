using CSPspEmu.Core.Cpu.Emitter;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Optimizers;
using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.InstructionCache
{
	public sealed class MethodCache
	{
		static public readonly MethodCache Methods = new MethodCache();

		private Dictionary<uint, MethodCacheInfo> MethodMapping = new Dictionary<uint, MethodCacheInfo>();

		public IEnumerable<uint> PCs { get { return MethodMapping.Keys; } }

		public MethodCache()
		{
		}

		static private AstMipsGenerator ast = AstMipsGenerator.Instance;

		static private readonly GeneratorIL GeneratorILInstance = new GeneratorIL();

		static private Action<CpuThreadState> GetGeneratorForPC(uint PC)
		{
			var Ast = ast.Statements(
				ast.Statement(ast.CallInstance(ast.CpuThreadState, (Action<MethodCacheInfo, uint>)CpuThreadState.Methods._MethodCacheInfo_SetInternal, ast.GetMethodCacheInfoAtPC(PC), PC)),
				ast.Statement(ast.CallTail(ast.CallInstance(ast.GetMethodCacheInfoAtPC(PC), (Action<CpuThreadState>)MethodCacheInfo.Methods.CallDelegate, ast.CpuThreadState))),
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
				return MethodMapping[PC] = new MethodCacheInfo()
				{
					MethodCache = this,
					StaticField = ILInstanceHolder.TAlloc<Action<CpuThreadState>>(DelegateGeneratorForPC),
				};
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
	}
}
