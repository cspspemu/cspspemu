using System;
using System.Reflection;
using System.Reflection.Emit;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
    public static class AstNodeExtensions
    {
        public static readonly GeneratorIlPsp GeneratorIlPsp = new GeneratorIlPsp();
        public static readonly GeneratorCSharpPsp GeneratorCSharpPsp = new GeneratorCSharpPsp();

        public static AstNodeStm Optimize(this AstNodeStm astNodeStm, CpuProcessor cpuProcessor) =>
            AstOptimizerPsp.GlobalOptimize(cpuProcessor, astNodeStm);

        public static TType GenerateDelegate<TType>(this AstNodeStm astNodeStm, string methodName) =>
            GeneratorIlPsp.GenerateDelegate<TType>(methodName, astNodeStm);

        public static void GenerateIl(this AstNodeStm astNodeStm, MethodInfo methodInfo, ILGenerator ilGenerator) =>
            GeneratorIlPsp.Init(methodInfo, ilGenerator).Reset().GenerateRoot(astNodeStm);

        public static void GenerateIl(this AstNodeStm astNodeStm, MethodInfo dynamicMethod)
        {
            ILGenerator ilGenerator;
            switch (dynamicMethod)
            {
                case DynamicMethod dm:
                    ilGenerator = dm.GetILGenerator();
                    break;
                case MethodBuilder mb:
                    ilGenerator = mb.GetILGenerator();
                    break;
                default:
                    throw(new InvalidOperationException("Not a DynamicMethod/MethodBuilder"));
            }
            GenerateIl(astNodeStm, dynamicMethod, ilGenerator);
        }

        public static void GenerateIl(this AstNodeStm astNodeStm, MethodBuilder dynamicMethod) =>
            GenerateIl(astNodeStm, dynamicMethod, dynamicMethod.GetILGenerator());

        public static string ToIlString<TDelegate>(this AstNodeStm astNodeStm) =>
            ToIlString(astNodeStm, typeof(TDelegate).GetMethod("Invoke"));

        public static string ToIlString(this AstNodeStm astNodeStm, MethodInfo methodInfo) =>
            GeneratorIlPsp.Reset().GenerateToString(methodInfo, astNodeStm);

        public static string ToCSharpString(this AstNode astNode) =>
            GeneratorCSharpPsp.GenerateRoot(astNode).ToString();
    }
}