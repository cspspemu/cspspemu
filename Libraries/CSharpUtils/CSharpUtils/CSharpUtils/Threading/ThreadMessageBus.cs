using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpUtils.Threading
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadMessageBus<T>
    {
        private LinkedList<T> Queue = new LinkedList<T>();
        private ManualResetEvent HasItems = new ManualResetEvent(false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Item"></param>
        public void AddFirst(T Item)
        {
            lock (this)
            {
                Queue.AddFirst(Item);
                HasItems.Set();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Item"></param>
        public void AddLast(T Item)
        {
            lock (this)
            {
                Queue.AddLast(Item);
                HasItems.Set();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T ReadOne()
        {
            HasItems.WaitOne();
            lock (this)
            {
                var Item = Queue.First.Value;
                Queue.RemoveFirst();
                if (Queue.Count == 0) HasItems.Reset();
                return Item;
            }
        }
    }
}