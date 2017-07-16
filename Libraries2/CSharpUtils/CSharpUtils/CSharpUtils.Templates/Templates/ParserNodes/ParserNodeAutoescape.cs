using CSharpUtils.Templates.ParserNodes;

namespace CSharpUtils.Templates.Templates.ParserNodes
{
	public class ParserNodeAutoescape : ParserNode
	{
		protected ParserNode AutoescapeExpression;
		protected ParserNode Body;

		public ParserNodeAutoescape(ParserNode AutoescapeExpression, ParserNode Body)
		{
			this.AutoescapeExpression = AutoescapeExpression;
			this.Body = Body;
		}

		public override void WriteTo(ParserNodeContext Context)
		{
			Context.Write("Autoescape(Context, ");
			AutoescapeExpression.WriteTo(Context);
			Context.WriteLine(", new EmptyDelegate(async delegate() {");
			Body.WriteTo(Context);
			Context.WriteLine("}));");
		}
	}
}
