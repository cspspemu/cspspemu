using System;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class LanguageUtils
    {
        /// <summary>
        /// Swaps the value of two references.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Swap<TType>(ref TType left, ref TType right)
        {
            var temp = left;
            left = right;
            right = temp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="copyToLeft"></param>
        public static void Transfer<TType>(ref TType left, ref TType right, bool copyToLeft)
        {
            if (copyToLeft)
            {
                left = right;
            }
            else
            {
                right = left;
            }
        }

        /// <summary>
        /// Changes the value of a reference just while the execution of the LocalScope delegate.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="variable"></param>
        /// <param name="localValue"></param>
        /// <param name="localScope"></param>
        public static void LocalSet<TType>(ref TType variable, TType localValue, Action localScope)
        {
            var oldValue = variable;
            // @TODO: A resharper bug?
            // ReSharper disable once RedundantAssignment
            variable = localValue;
            try
            {
                localScope();
            }
            finally
            {
                variable = oldValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="propertyName"></param>
        /// <param name="localValue"></param>
        /// <param name="localScope"></param>
        public static void PropertyLocalSet(object Object, string propertyName, object localValue, Action localScope)
        {
            var property = Object.GetType().GetProperty(propertyName);
            var oldValue = property?.GetValue(Object);
            property?.SetValue(Object, localValue);
            try
            {
                localScope();
            }
            finally
            {
                property?.SetValue(Object, oldValue);
            }
        }
    }
}