using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.GL.Utils
{
	public class GLShaderFilter : IDisposable
	{
		private GLRenderTarget RenderTarget;
		private GLBuffer PositionBuffer;
		private GLBuffer TexcoordsBuffer;
		private GLShader Shader;

		public int Width { get { return (RenderTarget != null) ? RenderTarget.Width : 0; } }
		public int Height { get { return (RenderTarget != null) ? RenderTarget.Height : 0; } }

		static public string DefaultVertexShader = @"
			attribute vec4 a_position;
			attribute vec4 a_texcoords;
			varying vec2 v_texcoords;
			void main() {
				gl_Position = a_position;
				v_texcoords = a_texcoords.xy;
			}
		";

		private GLShaderFilter(int Width, int Height, GLShader Shader)
		{
			this.Shader = Shader;
			SetSize(Width, Height);
			this.PositionBuffer = GLBuffer.Create().SetData<float>(new float[] {
				-1f, -1f,
				+1f, -1f,
				-1f, +1f,
				+1f, +1f,
			});
			this.TexcoordsBuffer = GLBuffer.Create().SetData<float>(new float[] {
				0f, 0f,
				1f, 0f,
				0f, 1f,
				1f, 1f,
			});
		}

		static public GLShaderFilter Create(int Width, int Height, GLShader Shader)
		{
			return new GLShaderFilter(Width, Height, Shader);
		}

		public GLTexture Process(Action<GLShader> Action)
		{
			RenderTarget.BindUnbind(() =>
			{
				GL.glClearStencil(0);
				GL.glClearDepthf(0f);
				GL.glClearColor(0, 0, 0, 1);
				GL.glClear(GL.GL_COLOR_CLEAR_VALUE | GL.GL_DEPTH_CLEAR_VALUE | GL.GL_STENCIL_CLEAR_VALUE);
				Shader.GetAttribute("a_position").NoWarning().SetData<float>(PositionBuffer, 2);
				Shader.GetAttribute("a_texcoords").NoWarning().SetData<float>(TexcoordsBuffer, 2);
				Shader.GetUniform("u_textureSize").NoWarning().Set(new Vector4f(Width, Height, 0, 0));
				Shader.GetUniform("u_pixelSize").NoWarning().Set(new Vector4f(1.0f / (float)Width, 1.0f / (float)Height, 0, 0));
				Shader.Draw(GLGeometry.GL_TRIANGLE_STRIP, 4, () =>
				{
					Action(Shader);
				});
			});
			return RenderTarget.TextureColor;
		}

		public GLShaderFilter SetSize(int Width, int Height)
		{
			if (Width != this.Width || Height != this.Height)
			{
				//Console.WriteLine("{0}x{1}", Width, Height);
				if (this.RenderTarget != null) this.RenderTarget.Dispose();
				this.RenderTarget = GLRenderTarget.Create(Width, Height, RenderTargetLayers.Color);
				//this.RenderTarget = GLRenderTarget.Create(Width, Height);
			}

			return this;
		}

		public void Dispose()
		{
			this.RenderTarget.Dispose();
			this.PositionBuffer.Dispose();
			this.TexcoordsBuffer.Dispose();
		}
	}
}
