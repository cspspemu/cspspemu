using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLES
{
	unsafe public class VertexAttribLocation
	{
		public uint Index;

		public VertexAttribLocation(uint Index)
		{
			this.Index = Index;
		}

		public void Enable()
		{
			GL.glEnableVertexAttribArray(this.Index);
		}

		public void Disable()
		{
			GL.glDisableVertexAttribArray(this.Index);
		}

		//[DllImport("libGLESv2")]
		//static public extern void glVertexAttribPointer(GLuint indx, GLint size, GLenum type, GLboolean normalized, GLsizei stride, void* ptr);

		public void Pointer(int size, int type, bool normalized, int stride, void* ptr)
		{
			GL.glVertexAttribPointer(this.Index, size, type, (normalized ? GL.GL_TRUE : GL.GL_FALSE), stride, ptr);
		}
	}
}
