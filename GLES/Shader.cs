using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLES
{
	unsafe public class Shader : IDisposable
	{
		public enum Type
		{
			Vertex,
			Fragment,
		}

		public uint Index;
		public Type ShaderType;

		public Shader(Type Type, string ShaderSource)
		{
			this.ShaderType = Type;

			// Create the shader object
			this.Index = GL.glCreateShader((Type == Shader.Type.Fragment) ? GL.GL_FRAGMENT_SHADER : GL.GL_VERTEX_SHADER);
			if (this.Index == 0) throw (new Exception("Can't create shader"));

			// Load the shader source
			int compiled = 0;
			var shaderSrcBytes = Encoding.UTF8.GetBytes(ShaderSource);
			fixed (byte* shaderSrcBytesPtr = shaderSrcBytes)
			{
				byte* shaderSrcBytesPtr2 = shaderSrcBytesPtr;
				GL.glShaderSource(this.Index, 1, &shaderSrcBytesPtr2, null);
			}

			// Compile the shader
			GL.glCompileShader(this.Index);

			// Check the compile status
			GL.glGetShaderiv(this.Index, GL.GL_COMPILE_STATUS, &compiled);

			if (compiled == 0)
			{
				string Info = GL.glGetShaderInfoLog(this.Index);
				GL.glDeleteShader(this.Index);

				throw (new Exception("glGetShaderInfoLog: " + Info));
			}

		}

		public void Dispose()
		{
#if false
			if (this.Index != 0)
			{
				GL.glDeleteShader(this.Index);
				this.Index = 0;
			}
#endif
		}
	}
}
