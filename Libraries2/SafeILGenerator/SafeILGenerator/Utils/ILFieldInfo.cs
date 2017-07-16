using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Utils
{
	public class ILFieldInfo
	{
		static private FieldInfo GetFieldInfo(Expression Expression)
		{
			switch (Expression.NodeType)
			{
				case ExpressionType.Lambda: return GetFieldInfo((Expression as LambdaExpression).Body);
				case ExpressionType.MemberAccess:
					return (FieldInfo)(Expression as MemberExpression).Member;
			}
			throw (new NotImplementedException("NodeType: " + Expression.NodeType));
		}

		static public FieldInfo GetFieldInfo<T>(Expression<Func<T>> Field)
		{
			return GetFieldInfo(Field as Expression);
		}
	}
}
