using System;
using System.Linq;
using System.Linq.Expressions;

namespace CSharpUtils.Containers.RedBlackTree
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TQElement"></typeparam>
    public class RedBlackTreeWithStatsQueryProvider<TQElement> : IQueryProvider
    {
        RedBlackTreeWithStats<TQElement>.Range _range;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        public RedBlackTreeWithStatsQueryProvider(RedBlackTreeWithStats<TQElement>.Range range)
        {
            _range = range;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="TElement"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var methodCallExpression = (expression as MethodCallExpression);
            if (methodCallExpression != null)
            {
                switch (methodCallExpression.Method.Name)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}