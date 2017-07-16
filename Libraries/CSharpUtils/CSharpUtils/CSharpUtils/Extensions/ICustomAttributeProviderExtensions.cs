using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetSingleAttribute<T>(this ICustomAttributeProvider methodInfo) where T : Attribute
        {
            return methodInfo.GetCustomAttributes(typeof(T), true).ElementAtOrDefault(0) as T;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetAttribute<T>(this ICustomAttributeProvider methodInfo) where T : Attribute
        {
            return methodInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
        }
    }
}