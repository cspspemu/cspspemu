using System;
using System.Threading;
using System.Diagnostics;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncTask<T>
    {
        protected Semaphore Semaphore;

        /// <summary>
        /// 
        /// </summary>
        public bool Ready { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        private T _result;

        /// <summary>
        /// 
        /// </summary>
        public T Result
        {
            get
            {
                Semaphore.WaitOne();
                Debug.Assert(Ready);
                return _result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getter"></param>
        public AsyncTask(Func<T> getter)
        {
            Semaphore = new Semaphore(0, 1);
            new Thread(delegate()
            {
                _result = getter();
                Ready = true;
                Semaphore.Release(1);
            }).Start();
            Thread.Yield();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator T(AsyncTask<T> that) => that.Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Result.ToString();
    }
}