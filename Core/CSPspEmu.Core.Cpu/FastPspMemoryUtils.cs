using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu
{
    public class FastPspMemoryUtils
    {
        private static AstGenerator ast = AstGenerator.Instance;

        private struct CacheKey
        {
            public IntPtr FixedGlobalAddress;
        }

        private static Dictionary<CacheKey, MethodInfo> Cache = new Dictionary<CacheKey, MethodInfo>();

        public static MethodInfo GetFastMemoryReader(IntPtr fixedGlobalAddress)
        {
            var cacheKey = new CacheKey {FixedGlobalAddress = fixedGlobalAddress};
            if (Cache.ContainsKey(cacheKey)) return Cache[cacheKey];
            const string dllName = "FastPspMemoryUtils_Gen.dll";
            const string typeName = "Memory";
            const string methodName = "Get";
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("FastPspMemoryUtils_Gen"), AssemblyBuilderAccess.RunAndCollect, dllName);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name, dllName, true);
            var typeBuilder = moduleBuilder.DefineType(typeName,
                TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.Class);
            var method = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Final | MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard, typeof(void*), new[] {typeof(uint)});
            method.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(MethodImplAttribute).GetConstructor(new[] {typeof(MethodImplOptions)}),
                new object[] {MethodImplOptions.AggressiveInlining}));
            method.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(TargetedPatchingOptOutAttribute).GetConstructor(new[] {typeof(string)}),
                new object[] {"Performance critical to inline across NGen image boundaries"}));
            //Method.GetILGenerator();

            var astTree = ast.Return(
                ast.Cast(
                    typeof(void*),
                    ast.Immediate(fixedGlobalAddress) + ast.Binary(ast.Argument<uint>(0), "&",
                        ast.Immediate(FastPspMemory.FastMemoryMask))
                )
            );

            new GeneratorIL().Reset().Init(method, method.GetILGenerator()).GenerateRoot(astTree);

            var type = typeBuilder.CreateType();
            Cache[cacheKey] = type.GetMethod(methodName);
            return Cache[cacheKey];
        }
    }
}