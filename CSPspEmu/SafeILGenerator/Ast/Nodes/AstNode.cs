using System.Collections.Generic;

namespace SafeILGenerator.Ast.Nodes
{
    public delegate AstNode TransformNodesDelegate(AstNode astNode);

    public static class TransformNodesDelegateExtensions
    {
        public static void Ref<T>(this TransformNodesDelegate transformer, ref T node) where T : AstNode
        {
            node = (T) transformer(node);
        }

        public static void Ref<T>(this TransformNodesDelegate transformer, ref List<T> nodes) where T : AstNode
        {
            var newNodes = new List<T>();
            foreach (var node in nodes)
            {
                var newNode = (T) transformer(node);
                if (newNode != null) newNodes.Add(newNode);
            }
            nodes = newNodes;
        }

        public static void Ref<T>(this TransformNodesDelegate transformer, ref T[] nodes) where T : AstNode
        {
            var newNodes = new List<T>();
            foreach (var node in nodes)
            {
                var newNode = (T) transformer(node);
                if (newNode != null) newNodes.Add(newNode);
            }
            nodes = newNodes.ToArray();
        }
    }

    public abstract class AstNode
    {
        public AstNode Parent;
        public abstract void TransformNodes(TransformNodesDelegate transformer);
        public virtual Dictionary<string, string> Info => null;

        public IEnumerable<AstNode> Descendant
        {
            get
            {
                foreach (var child in Childs)
                {
                    yield return child;
                    foreach (var grandChild in child.Descendant)
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        public IEnumerable<AstNode> Childs
        {
            get
            {
                var list = new List<AstNode>();
                TransformNodes(node =>
                {
                    list.Add(node);
                    return node;
                });
                return list;
            }
        }
    }
}