using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Ast.Serializers
{
	public class AstSerializer
	{
		static public string Serialize(AstNode Node)
		{
			var Parameters = new List<string>();
			foreach (var Child in Node.Childs)
			{
				Parameters.Add(Serialize(Child));
			}
			return Node.GetType().Name + "(" + String.Join(", ", Parameters) + ")";
		}

		static public string SerializeAsXml(AstNode Node, bool Spaces = true)
		{
			var Out = new IndentedStringBuilder();
			SerializeAsXml(Node, Out, Spaces);
			return Out.ToString();
		}

		static private void SerializeAsXml(AstNode Node, IndentedStringBuilder Out, bool Spaces)
		{
			var NodeName = Node.GetType().Name;
			Out.Write("<" + NodeName);
			var Parameters = Node.Info;
			if (Parameters != null)
			{
				foreach (var Pair in Parameters)
				{
					Out.Write(String.Format(" {0}=\"{1}\"", Pair.Key, Pair.Value));
				}
			}
			if (Node.Childs.Count() > 0)
			{
				Out.Write(">");
				if (Spaces) Out.WriteNewLine();
				if (Spaces)
				{
					Out.Indent(() =>
					{
						foreach (var Child in Node.Childs) SerializeAsXml(Child, Out, Spaces);
					});
				}
				else
				{
					foreach (var Child in Node.Childs) SerializeAsXml(Child, Out, Spaces);
				}
				Out.Write("</" + NodeName + ">");
				if (Spaces) Out.WriteNewLine();
			}
			else
			{
				Out.Write(" />");
				if (Spaces) Out.WriteNewLine();
			}
		}
	}
}
