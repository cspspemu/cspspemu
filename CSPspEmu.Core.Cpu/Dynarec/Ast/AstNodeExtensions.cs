using CSPspEmu.Core.Cpu.Dynarec.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu
{
	public static class AstNodeExtensions
	{
		static public AstNodeStm Optimize(this AstNodeStm AstNodeStm, CpuProcessor CpuProcessor)
		{
			return AstOptimizerPsp.GlobalOptimize(CpuProcessor, AstNodeStm);
		}

		static public string ToILString(this AstNodeStm AstNodeStm, MethodInfo MethodInfo)
		{
			return GeneratorIL.GenerateToString<GeneratorILPsp>(MethodInfo, AstNodeStm);
		}

		static public string ToCSharpString(this AstNode AstNode)
		{
			return new GeneratorCSharpPsp().GenerateRoot(AstNode).ToString();
		}
	}
}
