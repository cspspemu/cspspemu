using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MiniGL
{
	unsafe public class GL
	{
		public static void ActiveTexture(TextureUnit textureUnit)
		{
			//IGL.glActiveTexture(textureUnit);
			//throw new NotImplementedException();
			Console.Error.WriteLine("Not implemented GL.ActiveTexture");
		}

		public static void MatrixMode(MatrixMode matrixMode) { IGL.glMatrixMode(matrixMode); }
		public static void LoadIdentity() { IGL.glLoadIdentity(); }
		public static void Scale(float x, float y, float z) { IGL.glScalef(x, y, z); }

		public static void DepthRange(float near, float far) { IGL.glDepthRange(near, far); }
		public static void Viewport(int x, int y, int w, int h) { IGL.glViewport(x, y, w, h); }
		public static void Translate(float x, float y, int z) { IGL.glTranslatef(x, y, z); }

		public static void AlphaFunc(AlphaFunction alphaFunction, float reference) { IGL.glAlphaFunc (alphaFunction, reference); }

		public static ErrorCode GetError() { return IGL.glGetError(); }

		public static void Begin(BeginMode BeginMode) { IGL.glBegin(BeginMode); }
		public static void End() { IGL.glEnd(); }

		public static void Enable(EnableCap enableCap) { IGL.glEnable(enableCap); }
		public static void Disable(EnableCap enableCap) { IGL.glDisable(enableCap); }

		public static void BlendColor(float Red, float Green, float Blue, float Alpha)
		{
			Console.Error.WriteLine("Not implemented blendcolor! : {0}, {1}, {2}, {3}", Red, Green, Blue, Alpha);
		}

		public static void BlendEquation(BlendEquationMode blendEquationMode)
		{
			Console.Error.WriteLine("Not implemented BlendEquation! : {0}", blendEquationMode);
		}

		public static void BlendFunc(BlendingFactorSrc blendingFactorSrc, BlendingFactorDest blendingFactorDest) { IGL.glBlendFunc(blendingFactorSrc, blendingFactorDest); }
		public static void DepthFunc(DepthFunction depthFunction) { IGL.glDepthFunc(depthFunction); }
		public static void DepthMask(bool p) { IGL.glDepthMask(p); }
		public static void BindTexture(TextureTarget textureTarget, int TextureId) { IGL.glBindTexture(textureTarget, TextureId); }
		public static void ColorMask(bool r, bool g, bool b, bool a) { IGL.glColorMask(r, g, b, a); }

		public static void ColorMaterial(MaterialFace materialFace, ColorMaterialParameter flags) { IGL.glColorMaterial(materialFace, flags); }
		public static void Material(MaterialFace materialFace, MaterialParameter materialParameter, float* p) { IGL.glMaterialfv(materialFace, materialParameter, p); }
		public static void Hint(HintTarget hintTarget, HintMode hintMode) { IGL.glHint(hintTarget, hintMode); }

		public static int GenTexture()
		{
			var textures = stackalloc int[1];
			IGL.glGenTextures(1, textures);
			return textures[0];
		}

		public static void Flush() { IGL.glFlush(); }

		public static void TexImage2D(TextureTarget textureTarget, int level, PixelInternalFormat pixelInternalFormat, int TextureWidth, int TextureHeight, int border, PixelFormat pixelFormat, PixelType pixelType, IntPtr intPtr)
		{
			IGL.glTexImage2D(textureTarget, level, pixelInternalFormat, TextureWidth, TextureHeight, border, pixelFormat, pixelType, intPtr);
		}

		public static void DrawPixels(int width, int height, PixelFormat format, PixelType type, IntPtr pixels) { IGL.glDrawPixels(width, height, format, type, pixels.ToPointer()); }
		public static void ReadPixels(int x, int y, int width, int height, PixelFormat format, PixelType type, IntPtr pixels) { IGL.glReadPixels(x, y, width, height, format, type, pixels.ToPointer()); }

		public static unsafe void MultMatrix(float* m) { IGL.glMultMatrixf(m); }
		public static void CullFace(CullFaceMode cullFaceMode) { IGL.glCullFace(cullFaceMode); }
		public static void DeleteTexture(int TextureId)
		{
			var Textures = stackalloc int[1];
			Textures[0] = TextureId;
			IGL.glDeleteTextures(1, Textures);
		}

		public static void Normal3(float x, float y, float z) { IGL.glNormal3f(x, y, z); }
		public static void TexCoord3(float x, float y, float z) { IGL.glTexCoord3f(x, y, z); }
		public static void Vertex3(float x, float y, float z) { IGL.glVertex3f(x, y, z); }

		public static void PixelStore(PixelStoreParameter pixelStoreParameter, int p) { IGL.glPixelStorei(pixelStoreParameter, p); }
		public static void WindowPos2(ushort x, int y) {
			//IGL.glWindowPos2i(x, y);
			Console.Error.WriteLine("Not implemented: IGL.glWindowPos2i({0}, {1})", x, y);
		}
		public static void PixelZoom(int x, int y) { IGL.glPixelZoom(x, y); }
		public static void Ortho(double left, double right, double bottom, double top, double zNear, double zFar) { IGL.glOrtho(left, right, bottom, top, zNear, zFar); }
		public static void ShadeModel(ShadingModel shadingModel) { IGL.glShadeModel(shadingModel); }
		public static void StencilFunc(StencilFunction stencilFunction, byte reference, byte mask) { IGL.glStencilFunc(stencilFunction, reference, mask); }
		public static void StencilOp(StencilOp fail, StencilOp zfail, StencilOp zpass) { IGL.glStencilOp(fail, zfail, zpass); }

		public static void TexEnv(TextureEnvTarget textureEnvTarget, TextureEnvParameter textureEnvParameter, System.Drawing.Color color)
		{
			var components = stackalloc float[4];
			components[0] = (float)color.R / 255f;
			components[1] = (float)color.G / 255f;
			components[2] = (float)color.B / 255f;
			components[3] = (float)color.A / 255f;
			IGL.glTexEnvfv(textureEnvTarget, textureEnvParameter, components);
		}

		public static void TexEnv(TextureEnvTarget textureEnvTarget, TextureEnvParameter textureEnvParameter, int p) { IGL.glTexEnvi(textureEnvTarget, textureEnvParameter, p); }
		public static void TexParameter(TextureTarget textureTarget, TextureParameterName textureParameterName, int p) { IGL.glTexParameteri (textureTarget, textureParameterName, p); }
		public static void Light(LightName LightName, LightParameter lightParameter, float* p) { IGL.glLightfv(LightName, lightParameter, p); }
		public static void Light(LightName LightName, LightParameter lightParameter, int p) { IGL.glLighti(LightName, lightParameter, p); }
		public static void LightModel(LightModelParameter lightModelParameter, int p) { IGL.glLightModeli(lightModelParameter, p); }
		public static void LightModel(LightModelParameter lightModelParameter, float* p) { IGL.glLightModelfv(lightModelParameter, p); }

		public static void Color4(float Red, float Green, float Blue, float Alpha) { IGL.glColor4f(Red, Green, Blue, Alpha); }
		public static void Color4(float* p) { IGL.glColor4f(p[0], p[1], p[2], p[3]); }

		/// <summary>
		/// http://www.opengl.org/wiki/Swap_Interval
		/// </summary>
		/// <param name="Set"></param>
		static public void VSync(bool Set)
		{
			//var Extensions = IGL.glGetString(StringTarget.Extensions);
			//var Test = IGL.wglGetProcAddress("wglSwapIntervalEXT");
			var Test = IGL.wglGetProcAddress("glBlendEquation");
			Console.WriteLine(Test);
			Console.ReadKey();

			//wglSwapIntervalEXT(0);
		}
	}

	unsafe internal class IGL
	{
		const string Library = "opengl32";
		const CallingConvention LibraryCallingConvention = CallingConvention.StdCall;

		[DllImport(Library, CallingConvention = LibraryCallingConvention)]
		extern static public void wglSwapIntervalEXT(HintTarget target, HintMode mode);

		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glHint(HintTarget target, HintMode mode);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glViewport(int x, int y, int width, int height);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glColor4f(float red, float green, float blue, float alpha);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glBegin(BeginMode mode);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glEnd();
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glEnable(EnableCap cap);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glDisable(EnableCap cap);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glMatrixMode(MatrixMode mode);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glLoadIdentity();
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glScalef(float x, float y, float z);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glTranslatef(float x, float y, float z);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glDepthRange(double near, double far);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public ErrorCode glGetError();
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glTexEnvfv(TextureEnvTarget textureEnvTarget, TextureEnvParameter textureEnvParameter, float *values);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glTexEnvi(TextureEnvTarget textureEnvTarget, TextureEnvParameter textureEnvParameter, int p);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glPixelStoref(PixelStoreParameter pname, float param);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glPixelStorei(PixelStoreParameter pname, int param);
		//[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glWindowPos2i(int x, int y);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glPixelZoom(float xfactor, float yfactor);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glAlphaFunc(AlphaFunction alphaFunction, float reference);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glGenTextures(int p, int* textures);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glFlush();
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glNormal3f(float x, float y, float z);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glTexCoord3f(float x, float y, float z);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glVertex3f(float x, float y, float z);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glOrtho(double left, double right, double bottom, double top, double zNear, double zFar);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glLighti(LightName LightName, LightParameter lightParameter, int p);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glLightModeli(LightModelParameter lightModelParameter, int p);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glDeleteTextures(int p, int* Textures);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glCullFace(CullFaceMode cullFaceMode);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glMultMatrixf(float* m);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glTexParameteri(TextureTarget textureTarget, TextureParameterName textureParameterName, int p);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glTexImage2D(TextureTarget textureTarget, int level, PixelInternalFormat pixelInternalFormat, int TextureWidth, int TextureHeight, int border, PixelFormat pixelFormat, PixelType pixelType, IntPtr intPtr);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glBlendFunc(BlendingFactorSrc blendingFactorSrc, BlendingFactorDest blendingFactorDest);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glDepthFunc(DepthFunction depthFunction);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glDepthMask(bool p);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glBindTexture(TextureTarget textureTarget, int TextureId);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glColorMask(bool r, bool g, bool b, bool a);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glColorMaterial(MaterialFace materialFace, ColorMaterialParameter flags);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glMaterialfv(MaterialFace materialFace, MaterialParameter materialParameter, float* p);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glStencilFunc(StencilFunction stencilFunction, byte reference, byte mask);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glShadeModel(ShadingModel shadingModel);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glStencilOp(StencilOp fail, StencilOp zfail, StencilOp zpass);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glLightfv(LightName LightName, LightParameter lightParameter, float* p);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glLightModelfv(LightModelParameter lightModelParameter, float* p);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glDrawPixels(int width, int height, PixelFormat format, PixelType type, void* pixels);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public void glReadPixels(int x, int y, int width, int height, PixelFormat format, PixelType type, void* pixels);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public IntPtr wglGetProcAddress(string Name);
		[DllImport(Library, CallingConvention = LibraryCallingConvention)] extern static public IntPtr glGetString(StringTarget stringTarget);
	}

	public enum StringTarget
	{
		Vendor     =  0x1F00,
		Renderer   =  0x1F01,
		Version    =  0x1F02,
		Extensions =  0x1F03,
	}
}
