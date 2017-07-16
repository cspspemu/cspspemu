using System.Collections.Generic;

namespace CSharpUtils.Arrays
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public interface IArray<TType> : IEnumerable<TType>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        TType this[int index] { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int Length { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TType[] GetArray();
    }
}