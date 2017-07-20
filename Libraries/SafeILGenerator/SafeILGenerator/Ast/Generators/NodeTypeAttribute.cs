using System;

namespace SafeILGenerator.Ast.Generators
{
    public sealed class NodeTypeAttribute : Attribute
    {
        public Type Type { get; set; }

        public NodeTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}