using System;
using System.IO;

namespace CSharpUtils.Templates.ParserNodes
{
	public class ParserNodeContext
	{
		protected bool ShouldWriteIndentation;
		protected int IndentationLevel;
		protected TextWriter TextWriter;
		protected TemplateFactory TemplateFactory;

		public ParserNodeContext(TextWriter TextWriter, TemplateFactory TemplateFactory)
		{
			this.TextWriter = TextWriter;
			this.TemplateFactory = TemplateFactory;
			this.IndentationLevel = 0;
			this.ShouldWriteIndentation = true;
		}

		public void Indent(Action Action)
		{
			IndentationLevel++;
			try
			{
				Action();
			}
			finally
			{
				IndentationLevel--;
			}
		}

		protected void _TryWriteIndentation()
		{
			if (ShouldWriteIndentation)
			{
				TextWriter.Write(new String('\t', IndentationLevel));
				ShouldWriteIndentation = false;
			}
		}

		public void WriteLine(String Text, params object[] Params)
		{
			Write(Text, Params);
			TextWriter.WriteLine("");
			ShouldWriteIndentation = true;
		}

		public void Write(String Text, params object[] Params)
		{
			_TryWriteIndentation();
			if (Params.Length > 0)
			{
				TextWriter.Write(Text, Params);
			}
			else
			{
				TextWriter.Write(Text);
			}
		}

		internal String _GetContextWriteMethod()
		{
			return "await Context.Output.WriteAsync";
		}

		internal string _GetContextWriteAutoFilteredMethod()
		{
			return "await Context.OutputWriteAutoFilteredAsync";
		}
	}
}
