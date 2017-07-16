using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

#if false
namespace CSharpUtilsTests.Templates.DynaGenTest
{
	[TestFixture]
	public class DynaGenTest
	{
		public static Type DynamicPointTypeGen()
		{

			Type pointType = null;
			Type[] ctorParams = new Type[] {typeof(int), typeof(int), typeof(int)};

			AppDomain myDomain = Thread.GetDomain();
			AssemblyName myAsmName = new AssemblyName();
			myAsmName.Name = "MyDynamicAssembly";

			AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder pointModule = myAsmBuilder.DefineDynamicModule("PointModule", "Point.dll");
			TypeBuilder pointTypeBld = pointModule.DefineType("Point", TypeAttributes.Public);

			FieldBuilder xField = pointTypeBld.DefineField("x", typeof(int), FieldAttributes.Public);
			FieldBuilder yField = pointTypeBld.DefineField("y", typeof(int), FieldAttributes.Public);
			FieldBuilder zField = pointTypeBld.DefineField("z", typeof(int), FieldAttributes.Public);


			Type objType = Type.GetType("System.Object");
			ConstructorInfo objCtor = objType.GetConstructor(new Type[0]);

			ConstructorBuilder pointCtor = pointTypeBld.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctorParams);
			ILGenerator ctorIL = pointCtor.GetILGenerator();

			// NOTE: ldarg.0 holds the "this" reference - ldarg.1, ldarg.2, and ldarg.3
			// hold the actual passed parameters. ldarg.0 is used by instance methods
			// to hold a reference to the current calling object instance. Static methods
			// do not use arg.0, since they are not instantiated and hence no reference
			// is needed to distinguish them. 

			ctorIL.Emit(OpCodes.Ldarg_0);

			// Here, we wish to create an instance of System.Object by invoking its
			// constructor, as specified above.

			ctorIL.Emit(OpCodes.Call, objCtor);

			// Now, we'll load the current instance ref in arg 0, along
			// with the value of parameter "x" stored in arg 1, into stfld.

			ctorIL.Emit(OpCodes.Ldarg_0);
			ctorIL.Emit(OpCodes.Ldarg_1);
			ctorIL.Emit(OpCodes.Stfld, xField);

			// Now, we store arg 2 "y" in the current instance with stfld.

			ctorIL.Emit(OpCodes.Ldarg_0);
			ctorIL.Emit(OpCodes.Ldarg_2);
			ctorIL.Emit(OpCodes.Stfld, yField);

			// Last of all, arg 3 "z" gets stored in the current instance.

			ctorIL.Emit(OpCodes.Ldarg_0);
			ctorIL.Emit(OpCodes.Ldarg_3);
			ctorIL.Emit(OpCodes.Stfld, zField);

			// Our work complete, we return.

			ctorIL.Emit(OpCodes.Ret);

			// Now, let's create three very simple methods so we can see our fields.

			string[] mthdNames = new string[] { "GetX", "GetY", "GetZ" };

			foreach (string mthdName in mthdNames)
			{
				MethodBuilder getFieldMthd = pointTypeBld.DefineMethod(mthdName, MethodAttributes.Public, typeof(int), null);
				ILGenerator mthdIL = getFieldMthd.GetILGenerator();

				mthdIL.Emit(OpCodes.Ldarg_0);
				switch (mthdName)
				{
					case "GetX": mthdIL.Emit(OpCodes.Ldfld, xField); break;
					case "GetY": mthdIL.Emit(OpCodes.Ldfld, yField); break;
					case "GetZ": mthdIL.Emit(OpCodes.Ldfld, zField); break;

				}
				mthdIL.Emit(OpCodes.Ret);

			}
			// Finally, we create the type.

			pointType = pointTypeBld.CreateType();

			// Let's save it, just for posterity.

			//myAsmBuilder.Save("Point.dll");

			return pointType;

		}

		[Test]
		public void TestMethod1()
		{
			Type myDynamicType = DynamicPointTypeGen();

			Assert.AreEqual("Point", myDynamicType.FullName);
			Assert.AreEqual("MyDynamicAssembly, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", myDynamicType.Assembly.ToString());
			Assert.AreEqual("AutoLayout, AnsiClass, Class, Public", myDynamicType.Attributes.ToString());
			Assert.AreEqual("PointModule", myDynamicType.Module.ToString());

			dynamic Point = Activator.CreateInstance(myDynamicType, 1, 2, 3);
			Assert.AreEqual(1, Point.GetX());
			Assert.AreEqual(2, Point.GetY());
			Assert.AreEqual(3, Point.GetZ());
		}
	}
}
#endif
