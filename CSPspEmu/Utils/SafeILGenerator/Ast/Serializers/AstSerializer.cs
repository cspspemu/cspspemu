using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Utils;
using System.Collections.Generic;
using System.Linq;

namespace SafeILGenerator.Ast.Serializers
{
    public class AstSerializer
    {
        public static string Serialize(AstNode node)
        {
            var parameters = new List<string>();
            foreach (var child in node.Childs)
            {
                parameters.Add(Serialize(child));
            }
            return node.GetType().Name + "(" + string.Join(", ", parameters) + ")";
        }

        public static string SerializeAsXml(AstNode node, bool spaces = true)
        {
            var Out = new IndentedStringBuilder();
            SerializeAsXml(node, Out, spaces);
            return Out.ToString();
        }

        private static void SerializeAsXml(AstNode node, IndentedStringBuilder Out, bool spaces)
        {
            var nodeName = node.GetType().Name;
            Out.Write("<" + nodeName);
            var parameters = node.Info;
            if (parameters != null)
            {
                foreach (var pair in parameters)
                {
                    Out.Write($" {pair.Key}=\"{pair.Value}\"");
                }
            }
            if (node.Childs.Any())
            {
                Out.Write(">");
                if (spaces) Out.WriteNewLine();
                if (spaces)
                {
                    Out.Indent(() =>
                    {
                        foreach (var child in node.Childs) SerializeAsXml(child, Out, spaces: true);
                    });
                }
                else
                {
                    foreach (var child in node.Childs) SerializeAsXml(child, Out, spaces: false);
                }
                Out.Write("</" + nodeName + ">");
                if (spaces) Out.WriteNewLine();
            }
            else
            {
                Out.Write(" />");
                if (spaces) Out.WriteNewLine();
            }
        }
    }
}