using System;
using System.Collections.Generic;

namespace CSharpUtils
{
    public class LinqEqualityComparer<TSource, TResult> : IEqualityComparer<TSource>
    {
        Func<TSource, TResult> Select;

        public LinqEqualityComparer(Func<TSource, TResult> Select)
        {
            this.Select = Select;
        }

        public bool Equals(TSource Left, TSource Right)
        {
            return Select(Left).Equals(Select(Right));
        }

        public int GetHashCode(TSource Item)
        {
            return Select(Item).GetHashCode();
        }
    }
}