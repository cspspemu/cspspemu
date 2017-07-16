using System;
using System.Collections.Generic;
using System.Reflection;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprFieldAccess : AstNodeExprLValue
    {
        public AstNodeExpr Instance;
        public FieldInfo Field;

        public AstNodeExprFieldAccess(AstNodeExpr instance, string fieldName)
            : this(instance, instance.Type.GetField(fieldName), fieldName)
        {
        }

        public AstNodeExprFieldAccess(AstNodeExpr instance, FieldInfo field, string fieldName = null)
        {
            if (field == null) throw (new Exception($"Field can't be null '{fieldName}'"));
            Instance = instance;
            Field = field;
        }

        protected override Type UncachedType => Field.FieldType;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
            transformer.Ref(ref Instance);
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Field", Field.Name},
        };
    }

    public class AstNodeExprStaticFieldAccess : AstNodeExprLValue
    {
        public FieldInfo Field;

        public AstNodeExprStaticFieldAccess(FieldInfo field, string fieldName = null)
        {
            if (field == null) throw (new Exception($"Field can't be null '{fieldName}'"));
            Field = field;
        }

        protected override Type UncachedType => Field.FieldType;

        public override void TransformNodes(TransformNodesDelegate transformer)
        {
        }

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Field", Field.DeclaringType?.Name + "." + Field.Name},
        };
    }
}