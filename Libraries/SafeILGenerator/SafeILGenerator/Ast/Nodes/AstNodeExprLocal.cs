﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprLocal : AstNodeExprLValue
    {
        public readonly AstLocal AstLocal;

        public AstNodeExprLocal(AstLocal astLocal)
        {
            this.AstLocal = astLocal;
        }

        protected override Type UncachedType => AstLocal.Type;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Local", AstLocal.Name},
        };
    }
}