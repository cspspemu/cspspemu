using System;
using System.Collections.Generic;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class AnonymousComparer<TType> : IComparer<TType>
    {
        protected Func<TType, TType, int> Delegate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Delegate"></param>
        public AnonymousComparer(Func<TType, TType, int> Delegate)
        {
            this.Delegate = Delegate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(TType x, TType y)
        {
            return Delegate(x, y);
        }
    }
}