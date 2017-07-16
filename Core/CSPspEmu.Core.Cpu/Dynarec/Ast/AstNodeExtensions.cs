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
    static public readonly GeneratorILPsp _GeneratorILPsp = new GeneratorILPsp();
    static public readonly GeneratorCSharpPsp _GeneratorCSharpPsp = new GeneratorCSharpPsp();

    static public AstNodeStm Optimize(this AstNodeStm AstNodeStm, CpuProcessor CpuProcessor)
    {
        return AstOptimizerPsp.GlobalOptimize(CpuProcessor, AstNodeStm);
    }

    static public TType GenerateDelegate<TType>(this AstNodeStm AstNodeStm, string MethodName)
    {
        return _GeneratorILPsp.GenerateDelegate<TType>(MethodName, AstNodeStm);
    }

    static public void GenerateIL(this AstNodeStm AstNodeStm, MethodInfo MethodInfo, ILGenerator ILGenerator)
    {
        _GeneratorILPsp.Init(MethodInfo, ILGenerator).Reset().GenerateRoot(AstNodeStm);
    }

    static public void GenerateIL(this AstNodeStm AstNodeStm, MethodInfo DynamicMethod)
    {
        ILGenerator ILGenerator = null;
        if (DynamicMethod is DynamicMethod) ILGenerator = ((DynamicMethod) DynamicMethod).GetILGenerator();
        if (DynamicMethod is MethodBuilder) ILGenerator = ((MethodBuilder) DynamicMethod).GetILGenerator();
        if (ILGenerator == null) throw(new InvalidOperationException("Not a DynamicMethod/MethodBuilder"));
        GenerateIL(AstNodeStm, DynamicMethod, ILGenerator);
    }

    static public void GenerateIL(this AstNodeStm AstNodeStm, MethodBuilder DynamicMethod)
    {
        GenerateIL(AstNodeStm, DynamicMethod, DynamicMethod.GetILGenerator());
    }

    static public string ToILString<TDelegate>(this AstNodeStm AstNodeStm)
    {
        return AstNodeExtensions.ToILString(AstNodeStm, typeof(TDelegate).GetMethod("Invoke"));
    }

    static public string ToILString(this AstNodeStm AstNodeStm, MethodInfo MethodInfo)
    {
        return _GeneratorILPsp.Reset().GenerateToString(MethodInfo, AstNodeStm);
    }

    static public string ToCSharpString(this AstNode AstNode)
    {
        return _GeneratorCSharpPsp.GenerateRoot(AstNode).ToString();
    }
}