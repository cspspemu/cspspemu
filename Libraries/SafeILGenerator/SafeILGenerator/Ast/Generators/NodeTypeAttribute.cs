using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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