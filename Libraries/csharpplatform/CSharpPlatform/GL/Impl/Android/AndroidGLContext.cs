using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.GL.Impl.Android
{
	public class AndroidGLContext : IGLContext
	{
		private int Display;
		private IntPtr WindowHandle;

		public AndroidGLContext(IntPtr WindowHandle)
		{
			this.WindowHandle = WindowHandle;
			this.Display = EGL.eglGetDisplay(EGL.EGL_DEFAULT_DISPLAY);
			//EGL.eglCreateContext(Display);
			throw new NotImplementedException();
		}

		static public AndroidGLContext FromWindowHandle(IntPtr WindowHandle)
		{
			return new AndroidGLContext(WindowHandle);
		}

		public GLContextSize Size
		{
			get { throw new NotImplementedException(); }
		}

		public IGLContext MakeCurrent()
		{
			throw new NotImplementedException();
		}

		public IGLContext ReleaseCurrent()
		{
			throw new NotImplementedException();
		}

		public IGLContext SwapBuffers()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
