using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this Type type, bool inherit) // if (T is Attribute)
        {
            return type.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit) // if (T is Attribute)
        {
            return type.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Interface"></param>
        /// <returns></returns>
        public static bool Implements(this Type type, Type Interface)
        {
            return type.GetInterfaces().Contains(Interface);
        }
    }
}