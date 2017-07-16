using System;
using System.Collections.Generic;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="key"></param>
        /// <param name="allocator"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> This, TKey key, Func<TValue> allocator)
        {
            return !This.TryGetValue(key, out TValue item) ? (This[key] = allocator()) : item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> This, TKey key) where TValue : new()
        {
            return This.GetOrCreate(key, () => new TValue());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> This, TKey key, TValue defaultValue)
        {
            return This.TryGetValue(key, out TValue item) ? item : defaultValue;
        }
    }
}