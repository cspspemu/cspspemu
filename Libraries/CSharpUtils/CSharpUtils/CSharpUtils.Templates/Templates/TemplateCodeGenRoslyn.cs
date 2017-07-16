using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
// Disabled until stable
#if ENABLE_ROSLYN
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace CSharpUtils.Templates.Templates
{
	public class TemplateCodeGenRoslyn : TemplateCodeGen
	{
		public TemplateCodeGenRoslyn(String TemplateString, TemplateFactory TemplateFactory = null)
			: base(TemplateString, TemplateFactory)
		{
		}

		protected override Type GetTemplateCodeTypeByCode(string Code)
		{
			var tree = SyntaxTree.ParseCompilationUnit(Code);

			var compilation = Compilation.Create(
				"calc.dll",
				options: new CompilationOptions(assemblyKind: AssemblyKind.DynamicallyLinkedLibrary),
				syntaxTrees: new[] { tree },
				references: new[] {
					new AssemblyFileReference(typeof(object).Assembly.Location),
					new AssemblyFileReference(System.Reflection.Assembly.GetAssembly(typeof(TemplateCodeGen)).Location)
				}
			);

			var Errors = String.Join("\n", compilation.GetDiagnostics().Select(Diagnostic => Diagnostic.ToString()));
			if (Errors.Length > 0) throw (new Exception(Errors));

			Assembly compiledAssembly;
			using (var stream = new MemoryStream())
			{
				EmitResult compileResult = compilation.Emit(stream);
				compiledAssembly = Assembly.Load(stream.GetBuffer());
			}

			return compiledAssembly.GetType("CompiledTemplate_TempTemplate");
		}
	}
}
#endif
