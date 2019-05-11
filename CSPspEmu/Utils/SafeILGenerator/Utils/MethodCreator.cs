using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace SafeILGenerator.Utils
{
    public class MethodCreator
    {
        public static TDelegate CreateDynamicMethod<TDelegate>(Module module, string name, bool disableOptimizations,
            Action<MethodInfo> createMethod)
        {
            if (disableOptimizations) module = null;
            if (module == null) module = Assembly.GetExecutingAssembly().ManifestModule;
            var delegateType = typeof(TDelegate).GetMethod("Invoke");
            var dynamicMethod = new DynamicMethod(
                name,
                delegateType.ReturnType,
                delegateType.GetParameters().Select(parameter => parameter.ParameterType).ToArray(),
                module,
                true
                //skipVisibility: false
            );
            createMethod(dynamicMethod);
            return (TDelegate) (object) dynamicMethod.CreateDelegate(typeof(TDelegate));
        }

        public static TDelegate CreateMethodInClass<TDelegate>(Module module, string name, bool disableOptimizations,
            Action<MethodInfo> createMethod)
        {
            var delegateType = typeof(TDelegate).GetMethod("Invoke");
            const string dllName = "_DynamicDll.dll";
            var dynamicAssemblyName = "DynamicAssembly";
            const string dynamicModuleName = "DynamicModule";
            var dynamicMethodName = name;

            //var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(dynamicAssemblyName), AssemblyBuilderAccess.RunAndCollect, dllName);
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(dynamicAssemblyName), AssemblyBuilderAccess.RunAndCollect);
            var mmodule = assembly.DefineDynamicModule(dynamicModuleName);
            var type = mmodule.DefineType("DynamicType");
            var method = type.DefineMethod(
                dynamicMethodName,
                MethodAttributes.Static | MethodAttributes.Public,
                delegateType.ReturnType,
                delegateType.GetParameters().Select(parameter => parameter.ParameterType).ToArray()
            );

            if (disableOptimizations)
            {
                method.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(MethodImplAttribute).GetConstructor(new[] {typeof(MethodImplOptions)}) ??
                    throw new Exception(),
                    new object[] {MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining})
                );
            }
            else
            {
                method.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(MethodImplAttribute).GetConstructor(new[] {typeof(MethodImplOptions)}) ??
                    throw new Exception(),
                    new object[] { })
                );
            }

            method.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(TargetedPatchingOptOutAttribute).GetConstructor(new[] {typeof(string)}) ?? throw new Exception(),
                new object[] {"Performance critical to inline across NGen image boundaries"}));

            createMethod(method);
            var createdType = type.CreateType();
            var runtimeMethod = createdType.GetMethod(dynamicMethodName);

            return (TDelegate) (object) runtimeMethod.CreateDelegate(typeof(TDelegate));
        }
    }
}