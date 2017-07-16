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
        /// <param name="item"></param>
        public void AddFirst(T item)
        {
            lock (this)
            {
                Queue.AddFirst(item);
                HasItems.Set();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void AddLast(T item)
        {
            lock (this)
            {
                Queue.AddLast(item);
                HasItems.Set();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T ReadOne()
        {
            lock (this)
            {
                HasItems.WaitOne();
                var item = Queue.First.Value;
                Queue.RemoveFirst();
                if (Queue.Count == 0) HasItems.Reset();
                return item;
            }
        }
    }
}