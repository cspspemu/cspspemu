using System.Collections.Generic;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class LinkedListExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int GetCountLock<T>(this LinkedList<T> list)
        {
            lock (list) return list.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RemoveFirstAndGet<T>(this LinkedList<T> list)
        {
            lock (list)
            {
                try
                {
                    return list.First.Value;
                }
                finally
                {
                    list.RemoveFirst();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RemoveLastAndGet<T>(this LinkedList<T> list)
        {
            lock (list)
            {
                try
                {
                    return list.Last.Value;
                }
                finally
                {
                    list.RemoveLast();
                }
            }
        }
    }
}