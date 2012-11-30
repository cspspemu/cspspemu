using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLES
{
	unsafe public class Uniform
	{
		public int Index;

		public Uniform(int Index)
		{
			this.Index = Index;
		}

		public void SetMatrix4(float* values)
		{
			GL.glUniformMatrix4fv(this.Index, 1, false, values);
		}

		public void SetMatrix4(Matrix4 Matrix)
		{
			SetMatrix4((float *)&Matrix.Row0);
		}

		public void SetMatrix4(float[] values)
		{
			fixed (float* valuesPtr = values)
			{
				SetMatrix4(valuesPtr);
			}
		}

		public void SetVec4(float* v)
		{
			GL.glUniform4fv(this.Index, 1, v);
		}

		public void SetVec4(float[] v)
		{
			fixed (float* vp = v)
			{
				SetVec4(vp);
			}
		}

		public void SetVec4(float x, float y, float z, float w)
		{
			SetVec4(new[] { x, y, z, w });
		}

		public void SetBool(bool Value)
		{
			SetInt(Value ? 1 : 0);
		}

		public void SetInt(int Value)
		{
			GL.glUniform1i(this.Index, Value);
		}
	}
}
