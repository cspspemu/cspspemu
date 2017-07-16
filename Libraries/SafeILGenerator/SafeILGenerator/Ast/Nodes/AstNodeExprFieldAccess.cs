using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprFieldAccess : AstNodeExprLValue
	{
		public AstNodeExpr Instance;
		public FieldInfo Field;

		public AstNodeExprFieldAccess(AstNodeExpr Instance, string FieldName)
			: this(Instance, Instance.Type.GetField(FieldName), FieldName)
		{
		}

		public AstNodeExprFieldAccess(AstNodeExpr Instance, FieldInfo Field, string FieldName = null)
		{
			if (Field == null) throw (new Exception(String.Format("Field can't be null '{0}'", FieldName)));
			this.Instance = Instance;
			this.Field = Field;
		}

		protected override Type UncachedType
		{
			get { return Field.FieldType; }
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
					{ "Field", Field.Name },
				};
			}
		}
	}

	public class AstNodeExprStaticFieldAccess : AstNodeExprLValue
	{
		public FieldInfo Field;

		public AstNodeExprStaticFieldAccess(FieldInfo Field, string FieldName = null)
		{
			if (Field == null) throw (new Exception(String.Format("Field can't be null '{0}'", FieldName)));
			this.Field = Field;
		}

		protected override Type UncachedType
		{
			get { return Field.FieldType; }
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
					{ "Field", Field.DeclaringType.Name + "." + Field.Name },
				};
			}
		}
	}

}
