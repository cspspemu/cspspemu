using System.Numerics;

namespace CSPspEmu.Utils
{
    static public class MatrixExt
    {
        static public Matrix4x4 Transpose(this Matrix4x4 that) => Matrix4x4.Transpose(that);
    }
}