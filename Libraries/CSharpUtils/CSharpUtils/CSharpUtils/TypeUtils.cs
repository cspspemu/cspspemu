using System;
using System.Collections.Generic;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class TypeUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IEnumerable<Type> GetTypesExtending(Type baseType)
        {
            throw(new NotImplementedException());
            /*
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(Assembly => Assembly.GetTypes())
                .Where(Type => BaseType.IsAssignableFrom(Type) && (Type != BaseType)).ToArray()
            ;
            */
        }
    }
}