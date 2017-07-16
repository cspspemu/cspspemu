using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Nodes
{
	public class AstNodeExprCallInstance : AstNodeExprCall
	{
		public AstNodeExpr Instance;

		public AstNodeExprCallInstance(AstNodeExpr Instance, Delegate Delegate, params AstNodeExpr[] Parameters)
			: this(Instance, Delegate.Method, Parameters)
		{
			
		}

		public AstNodeExprCallInstance(AstNodeExpr Instance, MethodInfo MethodInfo, params AstNodeExpr[] Parameters)
			: base(MethodInfo, Parameters)
		{
			this.Instance = Instance;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref Instance);
			base.TransformNodes(Transformer);
		}
	}

	public class AstNodeExprCallDelegate : AstNodeExprCallInstance
	{
		private static MethodInfo GetInvoke(Type Type)
		{
			var MethodInfo = Type.GetMethod("Invoke");
			if (MethodInfo == null) throw(new Exception(String.Format("Can't get Invoke method for Type {0}", Type)));
			return MethodInfo;
		}

		public AstNodeExprCallDelegate(AstNodeExpr Object, params AstNodeExpr[] Parameters)
			: base(Object, GetInvoke(Object.Type), Parameters)
		{

		}
	}

}
