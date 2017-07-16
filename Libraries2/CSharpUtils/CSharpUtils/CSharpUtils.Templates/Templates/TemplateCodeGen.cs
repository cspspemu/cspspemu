using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using CSharpUtils.Templates.ParserNodes;
using CSharpUtils.Templates.Runtime;
using CSharpUtils.Templates.Tokenizers;
using CSharpUtils.Templates.Utils;
using Microsoft.CSharp;
#if ENABLE_ROSLYN
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers;
#endif

namespace CSharpUtils.Templates
{
	public class TemplateCodeGen
	{
		TemplateFactory TemplateFactory;
		TokenReader Tokens;
		public bool OutputGeneratedCode = false;

		public TemplateCodeGen(String TemplateString, TemplateFactory TemplateFactory = null)
		{
			this.TemplateFactory = TemplateFactory;
			this.Tokens = new TokenReader(TemplateTokenizer.Tokenize(new TokenizerStringReader(TemplateString)));
		}

		protected String GetCode()
		{
			StringWriter CodeWriter = new StringWriter();
			RenderCodeTo(CodeWriter);

			//Console.WriteLine(CodeWriter.ToString());

			return CodeWriter.ToString();
		}

		protected void RenderCodeTo(TextWriter TextWriter)
		{
			var TemplateHandler = new TemplateParser(Tokens, TextWriter);
			var Context = new ParserNodeContext(TextWriter, TemplateFactory);
			TemplateHandler.Reset();
			var ParserNode = TemplateHandler.HandleLevel_Root();

			//OptimizedParserNode.Dump();
			Context.WriteLine("using System;");
			Context.WriteLine("using System.Collections.Generic;");
			Context.WriteLine("using System.Threading.Tasks;");
			Context.WriteLine("using CSharpUtils.Templates;");
			Context.WriteLine("using CSharpUtils.Templates.Runtime;");
			Context.WriteLine("using CSharpUtils.Templates.TemplateProvider;");
			Context.WriteLine("");
			//Context.WriteLine("namespace CSharpUtils.Templates.CompiledTemplates {");
			Context.Indent(delegate()
			{
				Context.WriteLine("class CompiledTemplate_TempTemplate : TemplateCode {");

				Context.Indent(delegate()
				{
					Context.WriteLine("public CompiledTemplate_TempTemplate(TemplateFactory TemplateFactory = null) : base(TemplateFactory) { }");
					Context.WriteLine("");

					Context.WriteLine("public override void SetBlocks(Dictionary<String, RenderDelegate> Blocks) {");
					Context.Indent(delegate()
					{
						foreach (var BlockPair in TemplateHandler.Blocks)
						{
							Context.WriteLine(String.Format("SetBlock(Blocks, {0}, Block_{1});", StringUtils.EscapeString(BlockPair.Key), BlockPair.Key));
						}
					});
					Context.WriteLine("}");
					Context.WriteLine("");

					Context.WriteLine("async protected override Task LocalRenderAsync(TemplateContext Context) {");
					Context.Indent(delegate()
					{
						ParserNode.OptimizeAndWrite(Context);
					});
					Context.WriteLine("}"); // Method

					foreach (var BlockPair in TemplateHandler.Blocks)
					{
						Context.WriteLine("");
						Context.WriteLine("public async Task Block_" + BlockPair.Key + "(TemplateContext Context) {");
						Context.Indent(delegate()
						{
							BlockPair.Value.OptimizeAndWrite(Context);
						});
						Context.WriteLine("}"); // Method
					}
				});

				Context.WriteLine("}"); // class
			});

			//Context.WriteLine("}"); // namespace
		}

		protected virtual Type GetTemplateCodeTypeByCode(String Code)
		{
			CSharpCodeProvider CSharpCodeProvider = new CSharpCodeProvider();
			//Console.WriteLine(Assembly.GetExecutingAssembly().FullName);

			CompilerResults CompilerResults = CSharpCodeProvider.CompileAssemblyFromSource(
				new CompilerParameters(new string[] {
					"System.dll",
					"Microsoft.CSharp.dll",
					"System.Core.dll",
					System.Reflection.Assembly.GetAssembly(typeof(TemplateCodeGen)).Location
				}),
				Code
			);

			if (OutputGeneratedCode)
			{
				Console.Error.WriteLine(Code);
			}

			if (CompilerResults.NativeCompilerReturnValue == 0)
			{
				Assembly assembly = CompilerResults.CompiledAssembly;
				Type Type = assembly.GetType("CompiledTemplate_TempTemplate");
				return Type;
			}
			else
			{
				Console.Error.WriteLine(Code);

				foreach (var Error in CompilerResults.Errors)
				{
					Console.Error.WriteLine("Error: " + Error);
				}

				throw (new Exception("Error Compiling"));
			}
		}

		public TemplateCode GetTemplateCode()
		{
			return (TemplateCode)Activator.CreateInstance(GetTemplateCodeType(), TemplateFactory);
		}

		public Type GetTemplateCodeType()
		{
			return GetTemplateCodeTypeByCode(GetCode());
		}

		/*
		public String RenderToString(dynamic Parameters = null)
		{
			var OutputWriter = new StringWriter();
			GetTemplateCodeTypeByCode(GetCode()).Render(new TemplateContext(OutputWriter, Parameters, TemplateFactory));
			return OutputWriter.ToString();
		}
		*/
	}

}
