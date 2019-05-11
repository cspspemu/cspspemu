using System;

namespace SafeILGenerator.Ast
{
    public sealed class AstLocal
    {
        public readonly string Name;
        public readonly Type Type;

        private AstLocal(Type type, string name)
        {
            Name = name;
            Type = type;
        }

        public static AstLocal Create(Type type, string name = "<Unknown>")
        {
            return new AstLocal(type, name);
        }

        public static AstLocal Create<TType>(string name = "<Unknown>")
        {
            return Create(typeof(TType), name);
        }
    }
}