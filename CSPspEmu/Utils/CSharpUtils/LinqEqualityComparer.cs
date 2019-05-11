using System;
using System.Collections.Generic;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class LinqEqualityComparer<TSource, TResult> : IEqualityComparer<TSource>
    {
        readonly Func<TSource, TResult> _select;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        public LinqEqualityComparer(Func<TSource, TResult> @select)
        {
            _select = @select;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public bool Equals(TSource left, TSource right)
        {
            return _select(left).Equals(_select(right));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int GetHashCode(TSource item)
        {
            return _select(item).GetHashCode();
        }
    }
}