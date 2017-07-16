using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprImm : AstNodeExpr
    {
        public new Type Type;
        public object Value;

        public AstNodeExprImm(object value, Type type = null)
        {
            Value = value;
            //if (Value is RuntimeTypeHandle) throw (new NotImplementedException("Value is RuntimeTypeHandle"));

            if (type == null && value != null)
            {
                if (value is Type)
                {
                    type = typeof(Type);
                }
                else
                {
                    type = value.GetType();
                }
            }

            if (type == null) type = typeof(object);

            Type = type;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
        }

        protected override Type UncachedType => Type;

        public static implicit operator AstNodeExprImm(int value)
        {
            return new AstNodeExprImm(value);
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>()
        {
            {"Value", $"{Value}"},
        };
    }

    public class AstNodeExprNull : AstNodeExpr
    {
        public readonly Type Type;

        public AstNodeExprNull(Type type)
        {
            Type = type;
        }

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
        }

        protected override Type UncachedType => Type;

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Type", $"{Type.Name}"},
        };
    }
}