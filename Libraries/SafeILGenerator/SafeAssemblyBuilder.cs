using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Codegen
{
	static public class SafeAssemblyExtensions
	{
		static public void AddCustomAttribute<TType>(this TypeBuilder TypeBuilder)
		{
			TypeBuilder.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(TType).GetConstructor(new System.Type[] { }),
				new object[] { }
			));
		}

		static public ModuleBuilder CreateModuleBuilder(this AssemblyBuilder AssemblyBuilder, string DllName)
		{
			return AssemblyBuilder.DefineDynamicModule(AssemblyBuilder.GetName().Name, DllName, true);
		}
	}

	public class SafeAssemblyUtils
	{
		static public AssemblyBuilder CreateAssemblyBuilder(string AssemblyName, string OutputPath)
		{
			return  AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName(AssemblyName),
				AssemblyBuilderAccess.RunAndSave,
				OutputPath
			);
		}
	}
#if false
	public class SafeAssemblyBuilder
	{
		public AssemblyBuilder AssemblyBuilder { get; private set; }
		public ModuleBuilder ModuleBuilder { get; private set; }
		public string AssemblyName { get; private set; }

		public SafeAssemblyBuilder(string AssemblyName)
		{
			this.AssemblyName = AssemblyName;

			AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName(AssemblyName),
				AssemblyBuilderAccess.RunAndSave,
				@"c:\temp"
			);

			ModuleBuilder = AssemblyBuilder.DefineDynamicModule(
				AssemblyName,
				true
			);
		}

		public void Save(string FileName)
		{
			AssemblyBuilder.Save(FileName, PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);
		}

		public SafeTypeBuilder CreateType(string Name, TypeAttributes TypeAttributes, Type ParentType = null)
		{
			return new SafeTypeBuilder(ModuleBuilder.DefineType(Name, TypeAttributes, ParentType));
		}
	}

	public class SafeTypeBuilder
	{
		public TypeBuilder TypeBuilder { get; private set; }

		internal SafeTypeBuilder(TypeBuilder TypeBuilder)
		{
			this.TypeBuilder = TypeBuilder;
		}

		public SafeFieldBuilder CreateField(string FieldName, Type Type, FieldAttributes FieldAttributes)
		{
			return new SafeFieldBuilder(TypeBuilder.DefineField(FieldName, Type, FieldAttributes));
		}

		public void AddCustomAttribute<TType>()
		{
			TypeBuilder.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(TType).GetConstructor(new System.Type[] { }),
				new object[] { }
			));
		}
	}

	public class SafeFieldBuilder
	{
		public FieldBuilder FieldBuilder { get; private set; }

		public SafeFieldBuilder(FieldBuilder FieldBuilder)
		{
			this.FieldBuilder = FieldBuilder;
		}
	}
#endif
}
