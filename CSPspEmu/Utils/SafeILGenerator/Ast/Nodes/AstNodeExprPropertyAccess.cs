using System;
using System.Collections.Generic;
using System.Reflection;

namespace SafeILGenerator.Ast.Nodes
{
    public class AstNodeExprPropertyAccess : AstNodeExprLValue
    {
        public AstNodeExpr Instance;
        public PropertyInfo Property;

        public AstNodeExprPropertyAccess(AstNodeExpr instance, string propertyName)
            : this(instance, instance.Type.GetProperty(propertyName), propertyName)
        {
        }

        public AstNodeExprPropertyAccess(AstNodeExpr instance, PropertyInfo property, string propertyName = null)
        {
            if (property == null) throw new Exception($"Property can't be null '{propertyName}'");
            Instance = instance;
            Property = property;
        }

        protected override Type UncachedType => Property.PropertyType;

        public override void TransformNodes(TransformNodesDelegate transformer) => transformer.Ref(ref Instance);

        public override Dictionary<string, string> Info => new Dictionary<string, string>
        {
            {"Field", Property.Name},
        };
    }
}