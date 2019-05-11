using SafeILGenerator.Ast.Nodes;
using System.Collections.Generic;

namespace SafeILGenerator.Ast
{
    public class AstFunction
    {
        public List<AstArgument> Arguments;
        public List<AstLocal> Locals;
        public List<AstLabel> Labels;
        public AstNodeStmContainer Root;
    }
}