using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers;
using System.Reflection;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Table
{
	/// <summary>
	/// Microsoft “Roslyn” CTP
	/// <see cref="http://www.microsoft.com/download/en/details.aspx?id=27746"/>
	/// </summary>
	public class RoslynLookupGenerator
	{
		public void GenerateSwitchCode(IEnumerable<InstructionInfo> InstructionInfoList)
		{
			var Switch = GenerateSwitchString(InstructionInfoList);
			var SwitchSyntaxTree = SyntaxTree.ParseCompilationUnit(@"
				namespace HandlerNamespace {
					public class Handler {
						static void Handle(uint Value) {
							" + Switch + @"
						}
					}
				}
			");
			var Mscorlib = new AssemblyFileReference(typeof(object).Assembly.Location);
			var SwitchCompilation = Compilation.Create(
				"SwitchCompilation",
				syntaxTrees: new[] { SwitchSyntaxTree },
				references: new[] { Mscorlib }
			);

			var CurrentAppDomain = AppDomain.CurrentDomain;
			var AssemblyBuilder = CurrentAppDomain.DefineDynamicAssembly(new AssemblyName("assembly999"), AssemblyBuilderAccess.RunAndSave);
			var ModuleBuilder = AssemblyBuilder.DefineDynamicModule("module999");
			var Result = SwitchCompilation.Emit(ModuleBuilder);

			foreach (var Diagnostic in Result.Diagnostics)
			{
				Console.WriteLine(Diagnostic);
			}
			
			Console.WriteLine("AA:" + ModuleBuilder.GetTypes().Length);
			//SwitchSyntaxTree.
		}

		public String GenerateSwitchString(IEnumerable<InstructionInfo> InstructionInfoList) {
			uint CommonMask = InstructionInfoList.Aggregate(0xFFFFFFFF, (Base, InstructionInfo) => Base & InstructionInfo.Mask);
			var MaskGroups = InstructionInfoList.GroupBy((InstructionInfo) => InstructionInfo.Value & CommonMask);

			String Switch = "";
			Switch += "switch (Value & " + CommonMask + ") {";
			foreach (var MaskGroup in MaskGroups.Select(MaskGroup => MaskGroup.ToArray()))
			{
				Switch += "case " + (MaskGroup[0].Value & CommonMask) + ":";
				if (MaskGroup.Length > 1)
				{
					Switch += GenerateSwitchString(MaskGroup);
				}
				else
				{
					Switch += "Handle();";
				}
				Switch += "break;";

			}
			Switch += "default: break;";
			Switch += "}";

			return Switch;
		}
	}
}
