using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CSPspEmu.Sandbox
{
	sealed public class Memory
	{
	}

	sealed public class Processor
	{
		public int R1, R2, R3, R4;
		public Memory Memory;
	}

	public delegate void ProcessorDelegate(Processor Processor);

	class Program
	{
		static void Sample(Processor Processor)
		{
			Processor.R1 = 1;
		}

		static public void Store(ILGenerator ILGenerator, int Reg1)
		{
			ILGenerator.Emit(OpCodes.Stfld, typeof(Processor).GetField("R" + Reg1));
		}

		static public void SetValue(ILGenerator ILGenerator, int Reg1, int Value)
		{
			ILGenerator.Emit(OpCodes.Ldarg_0);
			//ILGenerator.Emit(OpCodes.Ldc_I4_1);
			ILGenerator.Emit(OpCodes.Ldc_I4, Value);
			Store(ILGenerator, Reg1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Common_Intermediate_Language"/>
		/// <see cref="http://en.wikipedia.org/wiki/List_of_CIL_instructions"/>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			var Method = new DynamicMethod("", typeof(void), new Type[] { typeof(Processor) });
			var ILGenerator = Method.GetILGenerator();

			SetValue(ILGenerator, 1, 1);
			SetValue(ILGenerator, 2, 2);
			SetValue(ILGenerator, 3, 3);
			SetValue(ILGenerator, 4, 4);
			/*
			ILGenerator.Emit(OpCodes.Ldarg_0);
			ILGenerator.Emit(OpCodes.Ldc_I4_1);
			ILGenerator.Emit(OpCodes.Stfld, typeof(Processor).GetField("R1"));
			*/

			ILGenerator.Emit(OpCodes.Ret);

			Console.WriteLine("[0]");
			ProcessorDelegate ProcessorFunction = (ProcessorDelegate)Method.CreateDelegate(typeof(ProcessorDelegate));
			Console.WriteLine("[1]");
			var Processor = new Processor();
			ProcessorFunction(Processor);
			Console.WriteLine(":: " + Processor.R1 + "," + Processor.R2 + "," + Processor.R3 + "," + Processor.R4);
			Console.WriteLine("[2]");
			Console.ReadKey();
		}
	}
}
