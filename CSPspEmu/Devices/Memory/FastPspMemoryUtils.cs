﻿using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;

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
            //var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("FastPspMemoryUtils_Gen"), AssemblyBuilderAccess.RunAndCollect, dllName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("FastPspMemoryUtils_Gen"), AssemblyBuilderAccess.RunAndCollect);
            
            //var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name, dllName, true);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().Name);
            var typeBuilder = moduleBuilder.DefineType(typeName,
                TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.Class);
            var method = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Final | MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard, typeof(void*), new[] {typeof(uint)});
            var constructorInfo = typeof(MethodImplAttribute).GetConstructor(new[] {typeof(MethodImplOptions)});
            method.SetCustomAttribute(new CustomAttributeBuilder(
                constructorInfo,
                new object[] {MethodImplOptions.AggressiveInlining}));

            var constructor = typeof(TargetedPatchingOptOutAttribute).GetConstructor(new[] {typeof(string)});
            method.SetCustomAttribute(new CustomAttributeBuilder(
                constructor,
                new object[] {"Performance critical to inline across NGen image boundaries"}));
            //Method.GetILGenerator();

            var astTree = ast.Return(
                ast.Cast(
                    typeof(void*),
                    ast.Immediate(fixedGlobalAddress) + ast.Binary(ast.Argument<uint>(0), "&",
                        ast.Immediate(FastPspMemory.FastMemoryMask))
                )
            );

            new GeneratorIl().Reset().Init(method, method.GetILGenerator()).GenerateRoot(astTree);

            var type = typeBuilder.CreateType();
            Cache[cacheKey] = type.GetMethod(methodName);
            return Cache[cacheKey];
        }
    }
}