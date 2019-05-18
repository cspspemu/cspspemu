using System;
using System.Numerics;

namespace CSPspEmu.Utils
{
    public static class VectorExt
    {
        public static Vector4 Normalize(this Vector4 vector) =>
            vector * (1.0f / (float) Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z));

        public static Vector3 ToRVector3(this Vector4 vector) => new Vector3(vector.X, vector.Y, vector.Z);
        public static Vector4 ToVector3(this Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z, 0f);
        public static Vector4 ToVector4(this Vector4 vector) => vector;
        //public static Vector4 ToVector4(this Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z, 1f);
        //public static Vector4 ToVector4(this Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z, 1f);
    }
}