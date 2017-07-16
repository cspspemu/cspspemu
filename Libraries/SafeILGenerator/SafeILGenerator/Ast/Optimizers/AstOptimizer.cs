using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Optimizers
{
	public class AstOptimizer
	{
		private Dictionary<Type, MethodInfo> GenerateMappings = new Dictionary<Type, MethodInfo>();

		public AstOptimizer()
		{
			foreach (
				var Method
				in
				this.GetType()
					.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.Where(Method => Method.ReturnType == typeof(AstNode))
					.Where(Method => Method.GetParameters().Count() == 1)
			)
			{
				GenerateMappings[Method.GetParameters().First().ParameterType] = Method;
			}
		}

		public AstNode Optimize(AstNode AstNode)
		{
			//if (AstNode != null)
			{
				//Console.WriteLine("Optimize.AstNode: {0}", AstNode);
				AstNode.TransformNodes(Optimize);

				var AstNodeType = AstNode.GetType();

				if (GenerateMappings.ContainsKey(AstNodeType))
				{
					AstNode = (AstNode)GenerateMappings[AstNodeType].Invoke(this, new[] { AstNode });
				}
				else
				{
					//throw(new NotImplementedException(String.Format("Don't know how to optimize {0}", AstNodeType)));
				}
			}
			
			return AstNode;
		}

		protected virtual AstNode _Optimize(AstNodeStmContainer Container)
		{
			if (Container.Nodes.Count == 1) return Container.Nodes[0];

			var NewContainer = new AstNodeStmContainer(Container.Inline);

			foreach (var Node in Container.Nodes)
			{
				if (Node == null) continue;

				if (Node is AstNodeStmContainer)
				{
					foreach (var Node2 in (Node as AstNodeStmContainer).Nodes)
					{
						if (!(Node2 is AstNodeStmEmpty))
						{
							NewContainer.AddStatement(Node2);
						}
					}
				}
				else
				{
					if (!(Node is AstNodeStmEmpty))
					{
						NewContainer.AddStatement(Node);
					}
				}
			}

			bool Rebuild = false;
			for (int n = 0; n < NewContainer.Nodes.Count - 1; n++)
			{
				var CurrentNode = NewContainer.Nodes[n];
				var NextNode = NewContainer.Nodes[n + 1];
				if ((CurrentNode is AstNodeStmGotoAlways) && (NextNode is AstNodeStmLabel))
				{
					if ((CurrentNode as AstNodeStmGotoAlways).AstLabel == (NextNode as AstNodeStmLabel).AstLabel)
					{
						NewContainer.Nodes[n] = null;
						//NewContainer.Nodes[n + 1] = null;
						Rebuild = true;
					}
				}
			}

			if (Rebuild)
			{
				return new AstNodeStmContainer(Container.Inline, NewContainer.Nodes.Where(Node => Node != null).ToArray());
			}
			else
			{
				return NewContainer;
			}
		}

		protected virtual AstNode _Optimize(AstNodeExprCast Cast)
		{
			//Console.WriteLine("Optimize.AstNodeExprCast: {0} : {1}", Cast.CastedType, Cast.Expr);

			// Dummy cast
			if (Cast.CastedType == Cast.Expr.Type)
			{
				//Console.WriteLine("Dummy Cast");
				return Cast.Expr;
			}
			// Double Cast
			else if (Cast.Expr is AstNodeExprCast)
			{
				//Console.WriteLine("Double Cast");
				var FirstCastType = (Cast.Expr as AstNodeExprCast).CastedType;
				var SecondCastType = Cast.CastedType;
				if (FirstCastType.IsPrimitive && SecondCastType.IsPrimitive)
				{
					if (AstUtils.GetTypeSize(FirstCastType) >= AstUtils.GetTypeSize(SecondCastType))
					{
						return Optimize(new AstNodeExprCast(Cast.CastedType, (Cast.Expr as AstNodeExprCast).Expr));
					}
				}
			}
			// Cast to immediate
			else if (Cast.Expr is AstNodeExprImm)
			{
				//Console.WriteLine("Cast to immediate");
				return new AstNodeExprImm(AstUtils.CastType((Cast.Expr as AstNodeExprImm).Value, Cast.CastedType));
			}

			return Cast;
		}

		protected virtual AstNode _Optimize(AstNodeExprImm Immediate)
		{
			return Immediate;
		}

		protected virtual AstNode _Optimize(AstNodeExprBinop Binary)
		{
			//Console.WriteLine("Optimize.AstNodeExprBinop: {0} {1} {2}", Binary.LeftNode, Binary.Operator, Binary.RightNode);
			var LeftImm = (Binary.LeftNode as AstNodeExprImm);
			var RightImm = (Binary.RightNode as AstNodeExprImm);
			var LeftType = Binary.LeftNode.Type;
			var RightType = Binary.RightNode.Type;
			var Operator = Binary.Operator;

			if ((LeftType == RightType))
			{
				if (AstUtils.IsTypeFloat(LeftType))
				{
					var Type = LeftType;

					if ((LeftImm != null) && (RightImm != null))
					{
						var LeftValue = Convert.ToDouble(LeftImm.Value);
						var RightValue = Convert.ToDouble(RightImm.Value);

						switch (Operator)
						{
							case "+": return new AstNodeExprImm(AstUtils.CastType(LeftValue + RightValue, Type));
							case "-": return new AstNodeExprImm(AstUtils.CastType(LeftValue - RightValue, Type));
							case "*": return new AstNodeExprImm(AstUtils.CastType(LeftValue * RightValue, Type));
							case "/": return new AstNodeExprImm(AstUtils.CastType(LeftValue / RightValue, Type));
						}
					}
					else if (LeftImm != null)
					{
						var LeftValue = Convert.ToInt64(LeftImm.Value);
						switch (Operator)
						{
							case "|": if (LeftValue == 0) return Binary.RightNode; break;
							case "+": if (LeftValue == 0) return Binary.RightNode; break;
							case "-": if (LeftValue == 0) return new AstNodeExprUnop("-", Binary.RightNode); break;
							case "*":
								//if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
								if (LeftValue == 1) return Binary.RightNode;
								break;
							case "/":
								//if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
								break;
						}
					}
					else if (RightImm != null)
					{
						var RightValue = Convert.ToInt64(RightImm.Value);
						switch (Operator)
						{
							case "|": if (RightValue == 0) return Binary.LeftNode; break;
							case "+": if (RightValue == 0) return Binary.LeftNode; break;
							case "-": if (RightValue == 0) return Binary.LeftNode; break;
							case "*":
								if (RightValue == 1) return Binary.LeftNode;
								break;
							case "/":
								//if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
								break;
						}
					}
				}
				else
				{
					if (Binary.RightNode is AstNodeExprUnop)
					{
						var RightUnary = Binary.RightNode as AstNodeExprUnop;
						if (Operator == "+" || Operator == "-")
						{
							if (RightUnary.Operator == "-")
							{
								return new AstNodeExprBinop(Binary.LeftNode, (Operator == "+") ? "-" : "+", RightUnary.RightNode);
							}
						}
					}

					var Type = LeftType;
					// Can optimize just literal values.
					if ((LeftImm != null) && (RightImm != null))
					{
						if (AstUtils.IsTypeSigned(LeftType))
						{
							var LeftValue = Convert.ToInt64(LeftImm.Value);
							var RightValue = Convert.ToInt64(RightImm.Value);

							switch (Operator)
							{
								case "+": return new AstNodeExprImm(AstUtils.CastType(LeftValue + RightValue, Type));
								case "-": return new AstNodeExprImm(AstUtils.CastType(LeftValue - RightValue, Type));
								case "*": return new AstNodeExprImm(AstUtils.CastType(LeftValue * RightValue, Type));
								case "/": return new AstNodeExprImm(AstUtils.CastType(LeftValue / RightValue, Type));
								case "<<": return new AstNodeExprImm(AstUtils.CastType(LeftValue << (int)RightValue, Type));
								case ">>": return new AstNodeExprImm(AstUtils.CastType(LeftValue >> (int)RightValue, Type));
							}
						}
						else
						{
							var LeftValue = Convert.ToUInt64(LeftImm.Value);
							var RightValue = Convert.ToUInt64(RightImm.Value);

							// Optimize adding 0
							switch (Operator)
							{
								case "+": return new AstNodeExprImm(AstUtils.CastType(LeftValue + RightValue, Type));
								case "-": return new AstNodeExprImm(AstUtils.CastType(LeftValue - RightValue, Type));
								case "*": return new AstNodeExprImm(AstUtils.CastType(LeftValue * RightValue, Type));
								case "/": return new AstNodeExprImm(AstUtils.CastType(LeftValue / RightValue, Type));
								case "<<": return new AstNodeExprImm(AstUtils.CastType(LeftValue << (int)RightValue, Type));
								case ">>": return new AstNodeExprImm(AstUtils.CastType(LeftValue >> (int)RightValue, Type));
							}
						}
					}
					else if (LeftImm != null)
					{
						var LeftValue = Convert.ToInt64(LeftImm.Value);
						switch (Operator)
						{
							case "&": if (LeftValue == 0) return new AstNodeExprImm(0); break;
							case "|": if (LeftValue == 0) return Binary.RightNode; break;
							case "+": if (LeftValue == 0) return Binary.RightNode; break;
							case "-": if (LeftValue == 0) return new AstNodeExprUnop("-", Binary.RightNode); break;
							case "*":
								//if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
								if (LeftValue == 1) return Binary.RightNode;
								break;
							case "/":
								//if (LeftValue == 0) return new AstNodeExprImm(AstUtils.CastType(0, Type));
								break;
						}
					}
					else if (RightImm != null)
					{
						var RightValue = Convert.ToInt64(RightImm.Value);
						switch (Operator)
						{
							case "0": if (RightValue == 0) return new AstNodeExprImm(0); break;
							case "|": if (RightValue == 0) return Binary.LeftNode; break;
							case "+":
								if (RightValue == 0) return Binary.LeftNode;
								if (RightValue < 0) return new AstNodeExprBinop(Binary.LeftNode, "-", new AstNodeExprImm(AstUtils.Negate(RightImm.Value)));
								break;
							case "-": if (RightValue == 0) return Binary.LeftNode; break;
							case "*":
								if (RightValue == 1) return Binary.LeftNode;
								break;
							case "/":
								//if (RightValue == 0) throw(new Exception("Can't divide by 0"));
								if (RightValue == 1) return Binary.LeftNode;
								break;
						}
					}
				} // !AstUtils.IsTypeFloat(LeftType)
			}

			// Special optimizations
			if ((LeftType == typeof(uint) || LeftType == typeof(int)) && RightType == typeof(int))
			{
				if (RightImm != null)
				{
					var RightValue = Convert.ToInt64(RightImm.Value);
					if (Operator == ">>" && (RightValue == 0)) return Binary.LeftNode;
				}
			}

			return Binary;
		}
	}
}
