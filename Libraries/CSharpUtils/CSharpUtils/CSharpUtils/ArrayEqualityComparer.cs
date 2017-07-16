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
        public bool Equals(T[] left, T[] right)
        {
            if (left == null || right == null) return left == right;
            return left.SequenceEqual(right);
        }

        public int GetHashCode(T[] key)
        {
            if (key == null) throw new ArgumentNullException("key");
            return key.Sum(Key => Key.GetHashCode());
        }
    }
}