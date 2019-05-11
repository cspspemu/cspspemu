using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SafeILGenerator.Utils
{
    public class IlFieldInfo
    {
        private static FieldInfo GetFieldInfo(Expression expression)
        {
            while (true)
            {
                switch (expression)
                {
                    case LambdaExpression expr:
                        expression = expr.Body;
                        continue;
                    case MemberExpression expr:
                        return (FieldInfo) expr.Member;
                }
                throw new NotImplementedException("NodeType: " + expression.NodeType);
            }
        }

        public static FieldInfo GetFieldInfo<T>(Expression<Func<T>> field) => GetFieldInfo(field as Expression);
    }
}