using System;
using System.Threading;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReaderWriterLockExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="callback"></param>
        public static void ReaderLock(this ReaderWriterLock This, Action callback)
        {
            This.AcquireReaderLock(int.MaxValue);
            try
            {
                callback();
            }
            finally
            {
                This.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="callback"></param>
        public static void WriterLock(this ReaderWriterLock This, Action callback)
        {
            This.AcquireWriterLock(int.MaxValue);
            try
            {
                callback();
            }
            finally
            {
                This.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ReaderLock<T>(this ReaderWriterLock This, Func<T> callback)
        {
            This.AcquireReaderLock(int.MaxValue);
            try
            {
                return callback();
            }
            finally
            {
                This.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T WriterLock<T>(this ReaderWriterLock This, Func<T> callback)
        {
            This.AcquireWriterLock(int.MaxValue);
            try
            {
                return callback();
            }
            finally
            {
                This.ReleaseWriterLock();
            }
        }
    }
}