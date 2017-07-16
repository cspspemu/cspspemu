using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform
{
	unsafe public delegate void CallbackFloatPointer(float* Values);

	unsafe public struct Matrix4f
	{
		public Vector4f Row0, Row1, Row2, Row3;

		public Vector4f Column(int n)
		{
			return Vector4f.Create(Row0[n], Row1[n], Row2[n], Row3[n]);
		}

		static public Matrix4f Create(params Vector4f[] Rows)
		{
			var Matrix = default(Matrix4f);
			for (int Row = 0; Row < 4; Row++)
			{
				(&Matrix.Row0)[Row] = Rows[Row];
			}
			return Matrix;
		}

		static public Matrix4f Create(params float[] Values)
		{
			var Matrix = default(Matrix4f);
			int n = 0;
			for (int Row = 0; Row < 4; Row++)
			{
				for (int Column = 0; Column < 4; Column++)
				{
					(&Matrix.Row0)[Row][Column] = Values[n++];
				}
			}
			return Matrix;
		}

		static public Matrix4f Identity
		{
			get
			{
				return Matrix4f.Create(
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1
				);
			}
		}

		static public Matrix4f Ortho(float left, float right, float bottom, float top, float near, float far)
		{
			var Matrix = default(Matrix4f);
			float rml = right - left;
			float fmn = far - near;
			float tmb = top - bottom;
			float _1over_rml;
			float _1over_fmn;
			float _1over_tmb;

			if (rml == 0.0 || fmn == 0.0 || tmb == 0.0) {
				throw(new Exception("Invalid matrix"));
			}

			_1over_rml = 1.0f / rml;
			_1over_fmn = 1.0f / fmn;
			_1over_tmb = 1.0f / tmb;

			Matrix.Row0 = Vector4f.Create(2.0f * _1over_rml, 0, 0, 0);
			Matrix.Row1 = Vector4f.Create(0, 2.0f * _1over_tmb, 0, 0);
			Matrix.Row2 = Vector4f.Create(0, 0, -2.0f * _1over_fmn, 0);
			Matrix.Row3 = Vector4f.Create(
				-(right + left) * _1over_rml,
				-(top + bottom) * _1over_tmb,
				-(far + near) * _1over_fmn,
				1.0f
			);

			return Matrix;
		}

		public float this[int Column, int Row]
		{
			get { fixed (Vector4f* RowsPtr = &Row0) return RowsPtr[Row][Column]; }
			set { fixed (Vector4f* RowsPtr = &Row0) RowsPtr[Row][Column] = value; }
		}

		public Matrix4f Multiply(Matrix4f that)
		{
			return StaticMultiply(this, that);
		}

		static public Matrix4f StaticMultiply(Matrix4f Left, Matrix4f Right)
		{
			var New = Matrix4f.Identity;
			for (int Column = 0; Column < 4; Column++)
			{
				for (int Row = 0; Row < 4; Row++)
				{
					float Dot = 0;
					for (int Index = 0; Index < 4; Index++) Dot += Left[Index, Row] * Right[Column, Index];
					New[Column, Row] = Dot;
				}
			}
			return New;
		}

		public Matrix4f Transpose()
		{
			return Matrix4f.Create(Column(0), Column(1), Column(2), Column(3));
		}

		public Matrix4f Translate(float X, float Y, float Z)
		{
			return StaticMultiply(this, Matrix4f.Create(
				1, 0, 0, X,
				0, 1, 0, Y,
				0, 0, 1, Z,
				0, 0, 0, 1
			));
		}

		public Matrix4f Scale(float X, float Y, float Z)
		{
			return StaticMultiply(this, Matrix4f.Create(
				X, 0, 0, 0,
				0, Y, 0, 0,
				0, 0, Z, 0,
				0, 0, 0, 1
			));
		}

		public void FixValues(CallbackFloatPointer Callback)
		{
			fixed (Vector4f* RowPtr = &this.Row0)
			{
				Callback((float*)RowPtr);
			}
		}

		public override string ToString()
		{
			return String.Format("Matrix4(\n  {0}\n  {1}\n  {2}\n  {3}\n)", Row0, Row1, Row2, Row3);
		}
	}
}
