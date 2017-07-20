using System.Collections.Generic;

namespace SafeILGenerator.Ast.Nodes
{
    public abstract class AstNodeStmGoto : AstNodeStm
    {
        public readonly AstLabel AstLabel;

        protected AstNodeStmGoto(AstLabel astLabel)
        {
            this.AstLabel = astLabel;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Label", AstLabel.Name},
        };
    }

    public abstract class AstNodeStmGotoIf : AstNodeStmGoto
    {
        public AstNodeExpr Condition;

        public AstNodeStmGotoIf(AstLabel astLabel, AstNodeExpr condition) : base(astLabel)
        {
            Condition = condition;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Condition);
        }
    }

    public class AstNodeStmGotoAlways : AstNodeStmGoto
    {
        public AstNodeStmGotoAlways(AstLabel astLabel)
            : base(astLabel)
        {
        }
    }

    public class AstNodeStmGotoIfTrue : AstNodeStmGotoIf
    {
        public AstNodeStmGotoIfTrue(AstLabel astLabel, AstNodeExpr condition)
            : base(astLabel, condition)
        {
        }
    }

    public class AstNodeStmGotoIfFalse : AstNodeStmGotoIf
    {
        public AstNodeStmGotoIfFalse(AstLabel astLabel, AstNodeExpr condition)
            : base(astLabel, condition)
        {
        }
    }
}