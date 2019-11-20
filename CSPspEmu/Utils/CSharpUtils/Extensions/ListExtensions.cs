using System;
using System.Collections.Generic;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortedAndNonRepeatedItems"></param>
        /// <param name="item"></param>
        /// <param name="lowerIndex"></param>
        /// <param name="higherIndex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int BoundIndex<T>(this List<T> sortedAndNonRepeatedItems, T item, int lowerIndex, int higherIndex)
            where T : IComparable
        {
            //Console.WriteLine("[{0}, {1}]", LowerIndex, HigherIndex);
            var index = lowerIndex;
            //var minIndex = lowerIndex;
            //var maxIndex = higherIndex;
            var maxIterations = 100;
            while (higherIndex > lowerIndex)
            {
                if (higherIndex - lowerIndex <= 2)
                {
                    lowerIndex++;
                }
                index = lowerIndex + (higherIndex - lowerIndex) / 2;
                var sign = sortedAndNonRepeatedItems[index].CompareTo(item);
                //Console.WriteLine(String.Format("Index: {0} [{1} - {2}] : {3}", Index, LowerIndex, HigherIndex, Sign));

                if (sign < 0)
                {
                    lowerIndex = index;
                }
                else if (sign > 0)
                {
                    higherIndex = index;
                }
                else
                {
                    break;
                }
                if (maxIterations-- <= 0)
                {
                    throw new Exception("Internal Error!");
                }
            }

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortedAndNonRepeatedItems"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int BoundIndex<T>(this List<T> sortedAndNonRepeatedItems, T item) where T : IComparable
        {
            return sortedAndNonRepeatedItems.BoundIndex(item, 0, sortedAndNonRepeatedItems.Count - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortedAndNonRepeatedItems"></param>
        /// <param name="item"></param>
        /// <param name="including"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> LowerBound<T>(this List<T> sortedAndNonRepeatedItems, T item,
            bool including = true)
            where T : IComparable
        {
            var index = Math.Min(sortedAndNonRepeatedItems.BoundIndex(item) + 1, sortedAndNonRepeatedItems.Count - 1);
            var compareValue = including ? 1 : 0;
            for (; index >= 0; index--)
            {
                //Console.WriteLine(Index);
                if (sortedAndNonRepeatedItems[index].CompareTo(item) < compareValue)
                {
                    yield return sortedAndNonRepeatedItems[index];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortedAndNonRepeatedItems"></param>
        /// <param name="item"></param>
        /// <param name="including"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> UpperBound<T>(this List<T> sortedAndNonRepeatedItems, T item,
            bool including = true)
            where T : IComparable
        {
            var index = Math.Max(sortedAndNonRepeatedItems.BoundIndex(item) - 1, 0);
            var compareValue = including ? -1 : 0;
            for (; index < sortedAndNonRepeatedItems.Count; index++)
            {
                if (sortedAndNonRepeatedItems[index].CompareTo(item) > compareValue)
                {
                    yield return sortedAndNonRepeatedItems[index];
                }
            }
        }
    }
}