using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu
{
	public class FastPspMemoryUtils
	{
		static private AstGenerator ast = AstGenerator.Instance;
		private struct CacheKey
		{
			public IntPtr FixedGlobalAddress;
		}
		static private Dictionary<CacheKey, MethodInfo> Cache = new Dictionary<CacheKey, MethodInfo>();

		static public MethodInfo GetFastMemoryReader(IntPtr FixedGlobalAddress)
		{
			var CacheKey = new CacheKey() { FixedGlobalAddress = FixedGlobalAddress };
			if (!Cache.ContainsKey(CacheKey))
			{
				var DllName = "FastPspMemoryUtils_Gen.dll";
				var TypeName = "Memory";
				var MethodName = "Get";
				var AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("FastPspMemoryUtils_Gen"), AssemblyBuilderAccess.RunAndCollect, DllName);
				var ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyBuilder.GetName().Name, DllName, true);
				var TypeBuilder = ModuleBuilder.DefineType(TypeName, TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.Class);
				var Method = TypeBuilder.DefineMethod(MethodName, MethodAttributes.Final | MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(void*), new[] { typeof(uint) });
				Method.SetCustomAttribute(new CustomAttributeBuilder(typeof(MethodImplAttribute).GetConstructor(new Type[] { typeof(MethodImplOptions) }), new object[] { MethodImplOptions.AggressiveInlining }));
				//Method.GetILGenerator();

				var AstTree = ast.Return(
					ast.Cast(
						typeof(void*),
						ast.Immediate(FixedGlobalAddress) + ast.Binary(ast.Argument<uint>(0), "&", ast.Immediate(FastPspMemory.FastMemoryMask))
					)
				);

				new GeneratorIL().Reset().Init(Method, Method.GetILGenerator()).GenerateRoot(AstTree);

				var Type = TypeBuilder.CreateType();
				Cache[CacheKey] = Type.GetMethod(MethodName);
			}
			return Cache[CacheKey];
		}
	}
}
