using System;
using System.Reflection;

namespace SafeILGenerator.Ast
{
    public class AstArgument
    {
        public readonly int Index;
        public readonly Type Type;
        public readonly string Name;

        public AstArgument(int index, Type type, string name = null)
        {
            Index = index;
            Type = type;
            Name = name ?? "@ARG(" + index + ")";
        }

        public static AstArgument Create(Type type, int index, string name = null) =>
            new AstArgument(index, type, name);

        public static AstArgument Create<TType>(int index, string name = null) => Create(typeof(TType), index, name);

        public static AstArgument Create(MethodInfo methodInfo, int index, string name = null)
        {
            if (name == null) name = methodInfo.GetParameters()[index].Name;
            return new AstArgument(index, methodInfo.GetParameters()[index].ParameterType, name);
        }
    }
}