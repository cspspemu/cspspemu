using System;
using System.Threading;

namespace CSPspEmu.Utils
{
    static public class SpanExt
    {
        public static unsafe Span<T> Reinterpret<T, R>(Span<R> Span) where T : unmanaged where R : unmanaged
        {
            fixed (R* bp = &Span.GetPinnableReference()) {
                //return new Span<T>(bp, count * sizeof(T));
                return new Span<T>(bp, Span.Length / sizeof(T));
            }
        }

    }
}