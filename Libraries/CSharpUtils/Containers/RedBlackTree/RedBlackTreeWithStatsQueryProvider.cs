using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CSharpUtils.Containers.RedBlackTree
{
	public class RedBlackTreeWithStatsQueryProvider<TQElement> : IQueryProvider
	{
		RedBlackTreeWithStats<TQElement>.Range Range;

		public RedBlackTreeWithStatsQueryProvider(RedBlackTreeWithStats<TQElement>.Range Range)
		{
			this.Range = Range;
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			var MethodCallExpression = (expression as MethodCallExpression);
			if (MethodCallExpression != null)
			{
				switch (MethodCallExpression.Method.Name)
				{
					case "Skip":
						/*
						var LambdaExpression = Expression.Lambda(MethodCallExpression.Arguments[0]);
						Func<int> d = LambdaExpression.Compile();
						int a = d();
						return Range.Skip(a);
						 * */
						break;
					default:
						throw new NotImplementedException();
				}
			}
			
			//Console.WriteLine((expression as MethodCallExpression).Method.Name);
			//Console.WriteLine(expression as Expression);
			throw new NotImplementedException();
		}

		public IQueryable CreateQuery(Expression expression)
		{
			throw new NotImplementedException();
		}

		public TResult Execute<TResult>(Expression expression)
		{
			throw new NotImplementedException();
		}

		public object Execute(Expression expression)
		{
			throw new NotImplementedException();
		}
	}
}
