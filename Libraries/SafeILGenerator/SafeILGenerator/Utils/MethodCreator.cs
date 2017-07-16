using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Utils
{
	public class MethodCreator
	{
		static public TDelegate CreateDynamicMethod<TDelegate>(Module Module, string Name, bool DisableOptimizations, Action<MethodInfo> CreateMethod)
		{
			if (DisableOptimizations) Module = null;
			if (Module == null) Module = Assembly.GetExecutingAssembly().ManifestModule;
			var DelegateType = typeof(TDelegate).GetMethod("Invoke");
			var DynamicMethod = new DynamicMethod(
				Name,
				DelegateType.ReturnType,
				DelegateType.GetParameters().Select(Parameter => Parameter.ParameterType).ToArray(),
				Module,
				true
				//skipVisibility: false
			);
			CreateMethod(DynamicMethod);
			return (TDelegate)(object)DynamicMethod.CreateDelegate(typeof(TDelegate));
		}

		static public TDelegate CreateMethodInClass<TDelegate>(Module Module, string Name, bool DisableOptimizations, Action<MethodInfo> CreateMethod)
		{
			var DelegateType = typeof(TDelegate).GetMethod("Invoke");
			var DllName = "_DynamicDll.dll";
			var DynamicAssemblyName = "DynamicAssembly";
			var DynamicModuleName = "DynamicModule";
			var DynamicMethodName = Name;

			var _Assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName(DynamicAssemblyName),
				AssemblyBuilderAccess.RunAndCollect,
				DllName
			);
			var _Module = _Assembly.DefineDynamicModule(DynamicModuleName);
			var _Type = _Module.DefineType("DynamicType");
			var Method = _Type.DefineMethod(
				DynamicMethodName,
				MethodAttributes.Static | MethodAttributes.Public,
				DelegateType.ReturnType,
				DelegateType.GetParameters().Select(Parameter => Parameter.ParameterType).ToArray()
			);

			if (DisableOptimizations)
			{
				Method.SetCustomAttribute(new CustomAttributeBuilder(typeof(MethodImplAttribute).GetConstructor(new Type[] { typeof(MethodImplOptions) }), new object[] { MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining }));
			}
			else
			{
				Method.SetCustomAttribute(new CustomAttributeBuilder(typeof(MethodImplAttribute).GetConstructor(new Type[] { typeof(MethodImplOptions) }), new object[] { }));
			}

			Method.SetCustomAttribute(new CustomAttributeBuilder(typeof(TargetedPatchingOptOutAttribute).GetConstructor(new Type[] { typeof(string) }), new object[] { "Performance critical to inline across NGen image boundaries" }));

			CreateMethod(Method);
			var CreatedType = _Type.CreateType();
			var RuntimeMethod = CreatedType.GetMethod(DynamicMethodName);

			return (TDelegate)(object)RuntimeMethod.CreateDelegate(typeof(TDelegate));
		}
	}
}
