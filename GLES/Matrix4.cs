using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLES
{
	unsafe public struct Matrix4
	{
		public fixed float Values[4 * 4];

		static public Matrix4 Create(float[] Values)
		{
			var Matrix = default(Matrix4);
			for (int n = 0; n < 4 * 4; n++) Matrix.Values[n] = Values[n];
			return Matrix;
		}

		static public Matrix4 Identity
		{
			get
			{
				return Matrix4.Create(new float[] {
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1,
				});
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

			Matrix.Values[0] = 2.0f * _1over_rml;
			Matrix.Values[1] = 0.0f;
			Matrix.Values[2] = 0.0f;
			Matrix.Values[3] = 0.0f;

			Matrix.Values[4] = 0.0f;
			Matrix.Values[5] = 2.0f * _1over_tmb;
			Matrix.Values[6] = 0.0f;
			Matrix.Values[7] = 0.0f;

			Matrix.Values[8] = 0.0f;
			Matrix.Values[9] = 0.0f;
			Matrix.Values[10] = -2.0f * _1over_fmn;
			Matrix.Values[11] = 0.0f;

			Matrix.Values[12] = -(right + left) * _1over_rml;
			Matrix.Values[13] = -(top + bottom) * _1over_tmb;
			Matrix.Values[14] = -(far + near) * _1over_fmn;
			Matrix.Values[15] = 1.0f;

			return Matrix;
		}

		public float this[int Row, int Column]
		{
			get
			{
				fixed (float* Values = this.Values)
				{
					return Values[Column * 4 + Row];
				}
			}
			set
			{
				fixed (float* Values = this.Values)
				{
					Values[Column * 4 + Row] = value;
				}
			}
		}

		static public Matrix4 Multiply(Matrix4 Left, Matrix4 Right)
		{
			var New = Matrix4.Identity;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					float accum = 0;
					for (int k = 0; k < 4; k++)
					{
						accum += Left[j, k] * Right[k, j];
					}
					New[i, j] = accum;
				}
			}
			return New;
		}

		public Matrix4 Translate(float X, float Y, int Z)
		{
			return Multiply(this, Matrix4.Create(new float[] {
				1, 0, 0, X,
				0, 1, 0, Y,
				0, 0, 1, Z,
				0, 0, 0, 1,
			}));
		}

		public Matrix4 Scale(float X, float Y, int Z)
		{
			return Multiply(this, Matrix4.Create(new float[] {
				X, 0, 0, 0,
				0, Y, 0, 0,
				0, 0, Z, 0,
				0, 0, 0, 1,
			}));
		}
	}
}
