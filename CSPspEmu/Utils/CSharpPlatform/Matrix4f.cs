using System;
using System.Numerics;
using CSPspEmu.Utils;

namespace CSharpPlatform
{
    public unsafe delegate void CallbackFloatPointer(float* values);

    public unsafe struct Matrix4F
    {
        public Vector4 Row0, Row1, Row2, Row3;

        public Vector4 Column(int n)
        {
            return new Vector4(Row0.Get(n), Row1.Get(n), Row2.Get(n), Row3.Get(n));
        }

        public static Matrix4F Create(params Vector4[] rows)
        {
            var matrix = default(Matrix4F);
            for (var row = 0; row < 4; row++)
            {
                (&matrix.Row0)[row] = rows[row];
            }
            return matrix;
        }

        public static Matrix4F Create(params float[] values)
        {
            var matrix = default(Matrix4F);
            var n = 0;
            for (var row = 0; row < 4; row++)
            {
                for (var column = 0; column < 4; column++)
                {
                    (&matrix.Row0)[row].Set(column, values[n++]);
                }
            }
            return matrix;
        }

        public static Matrix4F Identity => Matrix4F.Create(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

        public static Matrix4F Ortho(float left, float right, float bottom, float top, float near, float far)
        {
            var matrix = default(Matrix4F);
            var rml = right - left;
            var fmn = far - near;
            var tmb = top - bottom;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (rml == 0.0 || fmn == 0.0 || tmb == 0.0)
                throw new Exception("Invalid matrix");

            var _1OverRml = 1.0f / rml;
            var _1OverFmn = 1.0f / fmn;
            var _1OverTmb = 1.0f / tmb;

            matrix.Row0 = new Vector4(2.0f * _1OverRml, 0, 0, 0);
            matrix.Row1 = new Vector4(0, 2.0f * _1OverTmb, 0, 0);
            matrix.Row2 = new Vector4(0, 0, -2.0f * _1OverFmn, 0);
            matrix.Row3 = new Vector4(
                -(right + left) * _1OverRml,
                -(top + bottom) * _1OverTmb,
                -(far + near) * _1OverFmn,
                1.0f
            );

            return matrix;
        }

        public float this[int column, int row]
        {
            get
            {
                fixed (Vector4* rowsPtr = &Row0) return rowsPtr[row].Get(column);
            }
            set
            {
                fixed (Vector4* rowsPtr = &Row0) rowsPtr[row].Set(column, value);
            }
        }

        public Matrix4F Multiply(Matrix4F that)
        {
            return StaticMultiply(this, that);
        }

        public static Matrix4F StaticMultiply(Matrix4F left, Matrix4F right)
        {
            var New = Identity;
            for (var column = 0; column < 4; column++)
            {
                for (var row = 0; row < 4; row++)
                {
                    float dot = 0;
                    for (var index = 0; index < 4; index++) dot += left[index, row] * right[column, index];
                    New[column, row] = dot;
                }
            }
            return New;
        }

        public Matrix4F Transpose() => Create(Column(0), Column(1), Column(2), Column(3));

        public Matrix4F Translate(float x, float y, float z) => StaticMultiply(this, Matrix4F.Create(
            1, 0, 0, x,
            0, 1, 0, y,
            0, 0, 1, z,
            0, 0, 0, 1
        ));

        public Matrix4F Scale(float x, float y, float z) => StaticMultiply(this, Matrix4F.Create(
            x, 0, 0, 0,
            0, y, 0, 0,
            0, 0, z, 0,
            0, 0, 0, 1
        ));

        public void FixValues(CallbackFloatPointer callback)
        {
            fixed (Vector4* rowPtr = &Row0)
            {
                callback((float*) rowPtr);
            }
        }

        public override string ToString() => $"Matrix4(\n  {Row0}\n  {Row1}\n  {Row2}\n  {Row3}\n)";
    }
}