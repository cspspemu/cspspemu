using System.Collections.Generic;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class Iterators
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static IEnumerable<int> IntRange(int @from, int to)
        {
            for (var n = @from; n <= to; n++)
            {
                yield return n;
            }
        }
    }
}