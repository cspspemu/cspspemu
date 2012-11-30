using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GLES;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Text;

namespace Tests.GLES
{
	[TestClass]
	unsafe public class UnitTest1
	{
		[TestMethod]
		unsafe public void TestMatrix()
		{
			Console.WriteLine(Matrix4.Identity.Translate(2, 2, 0));
		}

		[TestMethod]
		unsafe public void TestMethod1()
		{
			int width = 512;
			int height = 272;
			var Context = new OffscreenContext(width, height);
			Context.SetCurrent();

			float[] vVertices = new float[]
			{
				0.0f,  0.5f, 0.0f, 
				-0.5f, -0.5f, 0.0f,
				0.5f, -0.5f, 0.0f
			};

			string vShaderStr = @"
				attribute vec4 vPosition;

				void main() {
					gl_Position = vPosition;
				}
			";
   
			string fShaderStr = @"
				precision mediump float;

				void main() {
					gl_FragColor = vec4 ( 1.0, 0.0, 0.0, 1.0 );
				}
			";

			// Load the vertex/fragment shaders
			var programObject = ShaderProgram.CreateProgram(vShaderStr, fShaderStr);
			var vPositionLocation = programObject.GetVertexAttribLocation("vPosition");
			programObject.Link();

			// Store the program object

			GL.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);

			GL.glViewport(0, 0, width, height);
			GL.glClear(GL.GL_COLOR_BUFFER_BIT);

			programObject.Use();

			// Load the vertex data
			fixed (float* vVerticesPtr = vVertices)
			{
				vPositionLocation.Pointer(3, GL.GL_FLOAT, false, 0, vVerticesPtr);
				vPositionLocation.Enable();

				GL.glDrawArrays(GL.GL_TRIANGLES, 0, 3);
			}

			//Context.SwapBuffers();

			var buffer = new byte[width * height * 4];

			fixed (byte* bufferPtr = buffer)
			{
				GL.glReadPixels(0, 0, width, height, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, bufferPtr);
			}

			File.WriteAllBytes(@"c:/temp/buffer.raw", buffer);

		}
	}
}
