using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprArgument : AstNodeExprLValue
	{
		public readonly AstArgument AstArgument;

		public AstNodeExprArgument(AstArgument AstArgument)
		{
			this.AstArgument = AstArgument;
		}

		protected override Type UncachedType
		{
			get { return AstArgument.Type; }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
		}

		public override Dictionary<string, string> Info
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Argument", AstArgument.Name },
				};
			}
		}
	}
}
