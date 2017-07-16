using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public abstract class AstNodeStmGoto : AstNodeStm
	{
		public readonly AstLabel AstLabel;

		public AstNodeStmGoto(AstLabel AstLabel)
		{
			this.AstLabel = AstLabel;
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
					{ "Label", AstLabel.Name },
				};
			}
		}
	}

	public abstract class AstNodeStmGotoIf : AstNodeStmGoto
	{
		public AstNodeExpr Condition;

		public AstNodeStmGotoIf(AstLabel AstLabel, AstNodeExpr Condition)
			: base (AstLabel)
		{
			this.Condition = Condition;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Condition);
		}
	}

	public class AstNodeStmGotoAlways : AstNodeStmGoto
	{
		public AstNodeStmGotoAlways(AstLabel AstLabel)
			: base(AstLabel)
		{
		}
	}

	public class AstNodeStmGotoIfTrue : AstNodeStmGotoIf
	{
		public AstNodeStmGotoIfTrue(AstLabel AstLabel, AstNodeExpr Condition)
			: base(AstLabel, Condition)
		{
		}
	}

	public class AstNodeStmGotoIfFalse : AstNodeStmGotoIf
	{
		public AstNodeStmGotoIfFalse(AstLabel AstLabel, AstNodeExpr Condition)
			: base(AstLabel, Condition)
		{
		}
	}
}
