using System.Collections.Generic;

namespace CSharpUtils.Arrays
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class ArrayWrapper<TType> : IArray<TType>
    {
        readonly TType[] _array;

        /// <summary>
        /// 
        /// </summary>
        public ArrayWrapper()
        {
            _array = new TType[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        public ArrayWrapper(TType[] array)
        {
            _array = array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public TType this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrayWrapper"></param>
        /// <returns></returns>
        public static implicit operator TType[](ArrayWrapper<TType> arrayWrapper) => arrayWrapper._array;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static implicit operator ArrayWrapper<TType>(TType[] array) => new ArrayWrapper<TType>(array);

        /// <summary>
        /// 
        /// </summary>
        public int Length => _array.Length;


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TType[] GetArray() => _array;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TType> GetEnumerator()
        {
            for (var n = 0; n < Length; n++) yield return this[n];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (var n = 0; n < Length; n++) yield return this[n];
        }
    }
}