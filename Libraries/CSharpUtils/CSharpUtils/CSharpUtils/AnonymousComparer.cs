using System;
using System.Collections.Generic;

namespace CSharpUtils
{
    public class AnonymousComparer<TType> : IComparer<TType>
    {
        protected Func<TType, TType, int> Delegate;

        public AnonymousComparer(Func<TType, TType, int> Delegate)
        {
            this.Delegate = Delegate;
        }

        public int Compare(TType x, TType y)
        {
            return this.Delegate(x, y);
        }
    }
}