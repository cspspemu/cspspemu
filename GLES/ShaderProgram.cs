using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLES
{
	unsafe public class ShaderProgram : IDisposable
	{
		public bool Linked = false;
		public uint Index = 0;
		public Shader FragmentShader;
		public Shader VertexShader;

		public ShaderProgram()
		{
			this.Index = GL.glCreateProgram();
			if (this.Index == 0) throw(new Exception("Can't create program shader"));
		}

		~ShaderProgram()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if (this.Index != 0)
			{
				if (this.VertexShader != null) this.VertexShader.Dispose();
				if (this.FragmentShader != null) this.FragmentShader.Dispose();
				GL.glDeleteProgram(this.Index);
				this.Index = 0;
				this.VertexShader = null;
				this.FragmentShader = null;
			}
		}

		public void AttachShader(Shader Shader)
		{
			switch (Shader.ShaderType)
			{
				case GLES.Shader.Type.Fragment: this.FragmentShader = Shader; break;
				case GLES.Shader.Type.Vertex: this.VertexShader = Shader; break;
			}
			GL.glAttachShader(this.Index, Shader.Index);
		}

		public void Link()
		{
			int linked = 0;
			GL.glLinkProgram(this.Index);

			// Check the link status
			GL.glGetProgramiv(this.Index, GL.GL_LINK_STATUS, &linked);
			this.Linked = (linked != 0);

			if (linked == 0)
			{
				GL.glDeleteProgram(this.Index);
				throw (new Exception(GL.glGetProgramInfoLog(this.Index)));
			}
		}

		public uint LastBindedAttribIndex = 0;

		public VertexAttribLocation GetVertexAttribLocation(string Name)
		{
			var AttribLocation = new VertexAttribLocation(this.LastBindedAttribIndex++);
			GL.glBindAttribLocation(this.Index, AttribLocation.Index, Name);
			return AttribLocation;
		}

		static public ShaderProgram CreateProgram(string VertexProgram, string FragmentProgram)
		{
			var vertexShader = new Shader(Shader.Type.Vertex, VertexProgram);
			var fragmentShader = new Shader(Shader.Type.Fragment, FragmentProgram);
			var programObject = new ShaderProgram();
			programObject.AttachShader(vertexShader);
			programObject.AttachShader(fragmentShader);
			return programObject;
		}

		public void Use()
		{
			if (!this.Linked) throw(new Exception("Program not linked"));
			GL.glUseProgram(this.Index);
		}

		public void Unuse()
		{
			if (!this.Linked) throw (new Exception("Program not linked"));
			GL.glUseProgram(this.Index);
		}
	}
}
