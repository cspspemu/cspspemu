//#define DEBUG_GENERATE_IL
//#define DEBUG_GENERATE_IL_CSHARP
#define USE_DYNAMIC_METHOD

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.Dynarec.Ast;

namespace CSPspEmu.Core.Cpu.Emitter
{
	/// <summary>
	/// <see cref="http://www.mrc.uidaho.edu/mrc/people/jff/digital/MIPSir.html"/>
	/// </summary>
	public unsafe partial class MipsMethodEmitter
	{
		public DynamicMethod DynamicMethod;
		protected String MethodName;
		public CpuProcessor Processor;
		public ILGenerator ILGenerator { get { return DynamicMethod.GetILGenerator(); } }
		
		public readonly Dictionary<string, uint> InstructionStats = new Dictionary<string, uint>();

		public MipsMethodEmitter(CpuProcessor Processor, uint PC, bool DoDebug = false, bool DoLog = false)
		{
			this.Processor = Processor;

			DynamicMethod = new DynamicMethod(
				String.Format("DynamicMethod_0x{0:X}", PC),
				typeof(void),
				new Type[] { typeof(CpuThreadState) },
				Assembly.GetExecutingAssembly().ManifestModule
			);
		}

		public Action<CpuThreadState> CreateDelegate(AstNodeStm AstNodeStm)
		{
			//ILGenerator.Emit(OpCodes.Ret);
			// Optimize
			AstNodeStm = AstOptimizerPsp.GlobalOptimize(Processor, ast.Statements(AstNodeStm, ast.Return()));

#if DEBUG_GENERATE_IL
			Console.WriteLine("{0}", GeneratorIL.GenerateToString<GeneratorILPsp>(DynamicMethod, AstNodeStm));
#endif
#if DEBUG_GENERATE_IL_CSHARP
			Console.WriteLine("{0}", (new GeneratorCSharpPsp()).GenerateRoot(AstNodeStm).ToString().Replace("CpuThreadState.", ""));
#endif

			new GeneratorILPsp().Init(DynamicMethod, ILGenerator).GenerateRoot(AstNodeStm);

			try
			{
				return (Action<CpuThreadState>)DynamicMethod.CreateDelegate(typeof(Action<CpuThreadState>));
			}
			catch (InvalidProgramException InvalidProgramException)
			{
#if LOG_TRACE
				Console.WriteLine("Invalid Delegate:");
				foreach (var Line in SafeILGenerator.GetEmittedInstructions())
				{
					if (Line.Substr(0, 1) == ":")
					{
						Console.WriteLine("{0}", Line);
					}
					else
					{
						Console.WriteLine("    {0}", Line);
					}
				}
#endif
				throw (InvalidProgramException);
			}
		}
	}
}
