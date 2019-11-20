using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Ext.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class LinqExExtensionsExt
    {
        /// <summary>
        /// http://msdn.microsoft.com/en-us/magazine/cc163329.aspx
        /// http://stackoverflow.com/questions/3789998/parallel-foreach-vs-foreachienumerablet-asparallel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this ParallelQuery<T> items, Action<T> action)
        {
            items.ForAll(action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="where"></param>
        /// <param name="compareValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int LocateWhereMinIndex<T>(this IEnumerable<T> items, Func<T, bool> where,
            Func<T, dynamic> compareValue)
        {
            var first = true;
            dynamic minValue = null;
            var minIndex = -1;
            var index = 0;
            foreach (var item in items)
            {
                if (where(item))
                {
                    dynamic curValue = compareValue(item);

                    if (first || curValue < minValue)
                    {
                        minValue = curValue;
                        minIndex = index;
                        first = false;
                    }
                }

                index++;
            }

            return minIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="compareValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LocateMin<T>(this IEnumerable<T> items, Func<T, dynamic> compareValue)
        {
            var first = true;
            var minItem = default(T);
            dynamic minValue = null;
            foreach (var item in items)
            {
                var curValue = compareValue(item);
                if (!first && curValue >= minValue) continue;
                minItem = item;
                minValue = curValue;
                first = false;
            }
            return minItem;
        }
    }
}