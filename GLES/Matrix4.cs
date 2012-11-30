using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLES
{
	unsafe public struct Vector4
	{
		public float x, y, z, w;

		public float this[int Index]
		{
			get { fixed (float* ValuesPtr = &x) return ValuesPtr[Index]; }
			set { fixed (float* ValuesPtr = &x) ValuesPtr[Index] = value; }
		}

		static public Vector4 Create(params float[] Values)
		{
			var Vector = default(Vector4);
			for (int n = 0; n < 4; n++) Vector[n] = Values[n];
			return Vector;
		}

		public void AddInplace(Vector4 Right)
		{
			for (int n = 0; n < 4; n++) this[n] = Right[n];
		}

		static public void Add(ref Vector4 Left, ref Vector4 Right, ref Vector4 Destination)
		{
			for (int n = 0; n < 4; n++) Destination[n] = Left[n] + Right[n];
		}

		public override string ToString()
		{
			return String.Format("Vector4({0}, {1}, {2}, {3})", x, y, z, w);
		}
	}

	unsafe public struct Matrix4
	{
		public Vector4 Row0, Row1, Row2, Row3;

		public Vector4 Column(int n)
		{
			return Vector4.Create(Row0[n], Row1[n], Row2[n], Row3[n]);
		}

		static public Matrix4 Create(params Vector4[] Rows)
		{
			var Matrix = default(Matrix4);
			for (int Row = 0; Row < 4; Row++)
			{
				(&Matrix.Row0)[Row] = Rows[Row];
			}
			return Matrix;
		}

		static public Matrix4 Create(params float[] Values)
		{
			var Matrix = default(Matrix4);
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

		static public Matrix4 Identity
		{
			get
			{
				return Matrix4.Create(
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1
				);
			}
		}

		static public Matrix4 Ortho(float left, float right, float bottom, float top, float near, float far)
		{
			var Matrix = default(Matrix4);
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

			Matrix.Row0 = Vector4.Create(2.0f * _1over_rml, 0, 0, 0);
			Matrix.Row1 = Vector4.Create(0, 2.0f * _1over_tmb, 0, 0);
			Matrix.Row2 = Vector4.Create(0, 0, -2.0f * _1over_fmn, 0);
			Matrix.Row3 = Vector4.Create(
				-(right + left) * _1over_rml,
				-(top + bottom) * _1over_tmb,
				-(far + near) * _1over_fmn,
				1.0f
			);

			return Matrix;
		}

		public float this[int Row, int Column]
		{
			get { fixed (Vector4* RowsPtr = &Row0) return RowsPtr[Row][Column]; }
			set { fixed (Vector4* RowsPtr = &Row0) RowsPtr[Row][Column] = value; }
		}

		static public Matrix4 Multiply(Matrix4 Left, Matrix4 Right)
		{
			var New = Matrix4.Identity;
			for (int Column = 0; Column < 4; Column++)
			{
				for (int Row = 0; Row < 4; Row++)
				{
					float Dot = 0;
					for (int Index = 0; Index < 4; Index++) Dot += Left[Row, Index] * Right[Index, Column];
					New[Row, Column] = Dot;
				}
			}
			return New;
		}

		public Matrix4 Transpose()
		{
			return Matrix4.Create(Column(0), Column(1), Column(2), Column(3));
		}

		public Matrix4 Translate(float X, float Y, int Z)
		{
			return Multiply(this, Matrix4.Create(
				1, 0, 0, X,
				0, 1, 0, Y,
				0, 0, 1, Z,
				0, 0, 0, 1
			));
		}

		public Matrix4 Scale(float X, float Y, int Z)
		{
			return Multiply(this, Matrix4.Create(
				X, 0, 0, 0,
				0, Y, 0, 0,
				0, 0, Z, 0,
				0, 0, 0, 1
			));
		}

		public override string ToString()
		{
			return String.Format("Matrix4(\n  {0}\n  {1}\n  {2}\n  {3}\n)", Row0, Row1, Row2, Row3);
		}
	}
}
