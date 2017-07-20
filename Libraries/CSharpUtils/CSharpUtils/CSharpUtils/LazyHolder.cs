using System;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <see cref="Lazy&lt;T&gt;"/>
    [Obsolete(message: "Use System.Lazy", error: false)]
    public class LazyHolder<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public Func<T> Getter;

        protected bool Setted;

        /// <summary>
        /// 
        /// </summary>
        public T _Value;

        /// <summary>
        /// 
        /// </summary>
        public T Value
        {
            get
            {
                if (!Setted)
                {
                    _Value = Getter();
                    Setted = true;
                }
                return _Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Invalidate()
        {
            Setted = false;
            //_Value = default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getter"></param>
        public LazyHolder(Func<T> getter)
        {
            Getter = getter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getter"></param>
        /// <returns></returns>
        public static LazyHolder<T> Create(Func<T> getter)
        {
            return new LazyHolder<T>(getter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lazyHolder"></param>
        /// <returns></returns>
        public static implicit operator T(LazyHolder<T> lazyHolder)
        {
            return lazyHolder.Value;
        }
    }
}