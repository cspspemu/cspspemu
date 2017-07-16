using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprLocal : AstNodeExprLValue
	{
		public readonly AstLocal AstLocal;

		public AstNodeExprLocal(AstLocal AstLocal)
		{
			this.AstLocal = AstLocal;
		}

		protected override Type UncachedType
		{
			get { return AstLocal.Type; }
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
					{ "Local", AstLocal.Name },
				};
			}
		}
	}
}
