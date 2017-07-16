using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils
{
    /// <summary>
    /// http://stackoverflow.com/questions/1440392/use-byte-as-key-in-dictionary
    /// </summary>
    public class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public bool Equals(T[] left, T[] right)
        {
            if (left == null || right == null) return left == right;
            return left.SequenceEqual(right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int GetHashCode(T[] key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return key.Sum(k => k.GetHashCode());
        }
    }
}