using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

public static class AstNodeExtensions
{
	public static readonly GeneratorILPsp GeneratorIlPsp = new GeneratorILPsp();
	public static readonly GeneratorCSharpPsp GeneratorCSharpPsp = new GeneratorCSharpPsp();

	public static AstNodeStm Optimize(this AstNodeStm astNodeStm, CpuProcessor cpuProcessor)
	{
		return AstOptimizerPsp.GlobalOptimize(cpuProcessor, astNodeStm);
	}

	public static TType GenerateDelegate<TType>(this AstNodeStm astNodeStm, string methodName)
	{
		return GeneratorIlPsp.GenerateDelegate<TType>(methodName, astNodeStm);
	}

	public static void GenerateIl(this AstNodeStm astNodeStm, MethodInfo methodInfo, ILGenerator ilGenerator)
	{
		GeneratorIlPsp.Init(methodInfo, ilGenerator).Reset().GenerateRoot(astNodeStm);
	}

	public static void GenerateIl(this AstNodeStm astNodeStm, MethodInfo dynamicMethod)
	{
		ILGenerator ilGenerator = null;
		if (dynamicMethod is DynamicMethod) ilGenerator = ((DynamicMethod)dynamicMethod).GetILGenerator();
		if (dynamicMethod is MethodBuilder) ilGenerator = ((MethodBuilder)dynamicMethod).GetILGenerator();
		if (ilGenerator == null) throw(new InvalidOperationException("Not a DynamicMethod/MethodBuilder"));
		GenerateIl(astNodeStm, dynamicMethod, ilGenerator);
	}

	public static void GenerateIl(this AstNodeStm astNodeStm, MethodBuilder dynamicMethod)
	{
		GenerateIl(astNodeStm, dynamicMethod, dynamicMethod.GetILGenerator());
	}

	public static string ToIlString<TDelegate>(this AstNodeStm astNodeStm)
	{
		return AstNodeExtensions.ToIlString(astNodeStm, typeof(TDelegate).GetMethod("Invoke"));
	}

	public static string ToIlString(this AstNodeStm astNodeStm, MethodInfo methodInfo)
	{
		return GeneratorIlPsp.Reset().GenerateToString(methodInfo, astNodeStm);
	}

	public static string ToCSharpString(this AstNode astNode)
	{
		return GeneratorCSharpPsp.GenerateRoot(astNode).ToString();
	}
}
