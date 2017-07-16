using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpUtils
{
    public class Iterators
    {
        static public IEnumerable<int> IntRange(int From, int To)
        {
            for (int n = From; n <= To; n++)
            {
                yield return n;
            }
        }
    }
}