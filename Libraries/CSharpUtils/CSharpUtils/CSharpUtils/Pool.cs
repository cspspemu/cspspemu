using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class Pool<TType>
    {
        private Queue<TType> FreeQueue = new Queue<TType>();
        private HashSet<TType> AllocatedHashSet = new HashSet<TType>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <param name="allocator"></param>
        public Pool(int count, Func<int, TType> allocator)
        {
            foreach (var item in Enumerable.Range(0, count).Select(index => allocator(index)))
            {
                FreeQueue.Enqueue(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Free(TType item)
        {
            AllocatedHashSet.Remove(item);
            FreeQueue.Enqueue(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TType Allocate()
        {
            var item = FreeQueue.Dequeue();
            AllocatedHashSet.Add(item);
            return item;
        }
    }
}