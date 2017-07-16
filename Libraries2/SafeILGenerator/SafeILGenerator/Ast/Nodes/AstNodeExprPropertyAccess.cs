using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprPropertyAccess : AstNodeExprLValue
	{
		public AstNodeExpr Instance;
		public PropertyInfo Property;

		public AstNodeExprPropertyAccess(AstNodeExpr Instance, string PropertyName)
			: this(Instance, Instance.Type.GetProperty(PropertyName), PropertyName)
		{
		}

		public AstNodeExprPropertyAccess(AstNodeExpr Instance, PropertyInfo Property, string PropertyName = null)
		{
			if (Property == null) throw (new Exception(String.Format("Property can't be null '{0}'", PropertyName)));
			this.Instance = Instance;
			this.Property = Property;
		}

		protected override Type UncachedType
		{
			get { return Property.PropertyType; }
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Instance);
		}

		public override Dictionary<string, string> Info
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Field", Property.Name },
				};
			}
		}
	}
}
