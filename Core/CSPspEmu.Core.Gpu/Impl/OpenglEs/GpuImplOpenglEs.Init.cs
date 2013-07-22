using CSharpUtils;
using GLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
	public sealed unsafe partial class GpuImplOpenglEs
	{
		public override void InitSynchronizedOnce()
		{
			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
			{
				Console.WriteLine("Gpu.InitSynchronizedOnce::Thread({0})", Thread.CurrentThread.ManagedThreadId);
			});

			var FragmentProgram = Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.OpenglEs.shader.fragment").ReadAllContentsAsString(Encoding.UTF8);
			var VertexProgram = Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.OpenglEs.shader.vertex").ReadAllContentsAsString(Encoding.UTF8);

			this.GraphicsContext = new OffscreenContext(512, 272);
			this.GraphicsContext.SetCurrent();

			this.glVENDOR = GL.glGetString2(GL.GL_VENDOR);
			this.glRENDERER = GL.glGetString2(GL.GL_RENDERER);
			this.glVERSION = GL.glGetString2(GL.GL_VERSION);
			this.glSLVERSION = GL.glGetString2(GL.GL_SHADING_LANGUAGE_VERSION);
			this.glEXTENSIONS = GL.glGetString2(GL.GL_EXTENSIONS);

			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
			{
				Console.WriteLine("{0}", this.glEXTENSIONS);
			});

			GL.glViewport(0, 0, 512, 272);
			{
				this.ShaderProgram = ShaderProgram.CreateProgram(VertexProgram, FragmentProgram);
				{
					this.aColorLocation = this.ShaderProgram.GetVertexAttribLocation("a_color0");
					this.aPositionLocation = this.ShaderProgram.GetVertexAttribLocation("a_position");
					this.aTexCoord = this.ShaderProgram.GetVertexAttribLocation("a_texCoord");
				}
				this.ShaderProgram.Link();
				{
					// Matrices
					this.projectionMatrix = this.ShaderProgram.GetUniformLocation("projectionMatrix");
					this.viewMatrix = this.ShaderProgram.GetUniformLocation("viewMatrix");
					this.worldMatrix = this.ShaderProgram.GetUniformLocation("worldMatrix");
					this.textureMatrix = this.ShaderProgram.GetUniformLocation("textureMatrix");

					// Colors
					this.fColor = this.ShaderProgram.GetUniformLocation("u_color");
					this.u_has_vertex_color = this.ShaderProgram.GetUniformLocation("u_has_vertex_color");
					this.u_transform_2d = this.ShaderProgram.GetUniformLocation("u_transform_2d");

					// Textures
					this.u_has_texture = this.ShaderProgram.GetUniformLocation("u_has_texture");
					this.u_texture = this.ShaderProgram.GetUniformLocation("u_texture");
					this.u_texture_effect = this.ShaderProgram.GetUniformLocation("u_texture_effect");
				}
			}

			//TestingRender();
		}

		public override void StopSynchronized()
		{
		}
	}
}
