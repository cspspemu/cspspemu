using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MiniGL
{
	public interface IGraphicsContext
	{
		void MakeCurrent(IWindowInfo IWindowInfo);
		bool VSync { get; set; }
		void SwapBuffers();
	}

	public interface INativeWindow
	{
		void ProcessEvents();
		IWindowInfo WindowInfo { get; }
	}

	public interface IWindowInfo
	{
	}

	public interface IGraphicsContextInternal
	{
		void LoadAll();
	}

	public class DisplayDevice
	{
		static public DisplayDevice Default
		{
			get
			{
				return new DisplayDevice();
			}
		}
	}

	public enum GameWindowFlags
	{
		Default = 0,
		Fullscreen = 1,
	}

	public class WindowInfo : IWindowInfo
	{
	}

	unsafe public class NativeWindow : INativeWindow
	{
		WindowInfo WindowInfo;

		public NativeWindow(int Width, int Height, string Title, GameWindowFlags GameWindowFlags, GraphicsMode GraphicsMode, DisplayDevice DisplayDevice)
		{
			//throw(new NotImplementedException());
			this.WindowInfo = new WindowInfo();
		}

		void INativeWindow.ProcessEvents()
		{
		}

		IWindowInfo INativeWindow.WindowInfo
		{
			get { return WindowInfo; }
		}

		public enum LRESULT : uint { }
		public enum WPARAM : uint { }
		public enum LPARAM : uint { }

		public enum HWND : uint { }
		public enum DWORD : uint { }
		public enum UINT : uint { }
		public enum WNDPROC : uint { }
		
		public enum WORD : ushort { }
		public enum BYTE : byte { }
		public enum HINSTANCE : uint { }
		public enum HDC : uint { }
		//public enum WNDCLASS : uint { }

		public enum HICON : uint { }
		public enum HCURSOR : uint { }
		public enum HBRUSH : uint { }
		public enum LPCTSTR : uint { }

		public struct PIXELFORMATDESCRIPTOR {
		  public WORD  nSize;
		  public WORD nVersion;
		  public DWORD dwFlags;
		  public BYTE iPixelType;
		  public BYTE cColorBits;
		  public BYTE cRedBits;
		  public BYTE cRedShift;
		  public BYTE cGreenBits;
		  public BYTE cGreenShift;
		  public BYTE cBlueBits;
		  public BYTE cBlueShift;
		  public BYTE cAlphaBits;
		  public BYTE cAlphaShift;
		  public BYTE cAccumBits;
		  public BYTE cAccumRedBits;
		  public BYTE cAccumGreenBits;
		  public BYTE cAccumBlueBits;
		  public BYTE cAccumAlphaBits;
		  public BYTE cDepthBits;
		  public BYTE cStencilBits;
		  public BYTE cAuxBuffers;
		  public BYTE iLayerType;
		  public BYTE bReserved;
		  public DWORD dwLayerMask;
		  public DWORD dwVisibleMask;
		  public DWORD dwDamageMask;
		}

		public struct WNDCLASS
		{
			public UINT style;
			public WNDPROC lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public HINSTANCE hInstance;
			public HICON hIcon;
			public HCURSOR hCursor;
			public HBRUSH hbrBackground;
			public LPCTSTR lpszMenuName;
			public LPCTSTR lpszClassName;
		}

		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		const string Library = "kernel32";
		const CallingConvention LibraryCallingConvention = CallingConvention.StdCall;

		[DllImport(Library, CallingConvention = LibraryCallingConvention)]
		extern static public bool RegisterClassA(WNDCLASS* ptr);

		[DllImport(Library, CallingConvention = LibraryCallingConvention)]
		extern static public HICON LoadIconA(void* a, char* b);

		[DllImport(Library, CallingConvention = LibraryCallingConvention)]
		extern static public HCURSOR LoadCursorA(void* a, char* b);

		[DllImport(Library, CallingConvention = LibraryCallingConvention)]
		extern static public LRESULT DefWindowProcA(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);
		
		[DllImport(Library, CallingConvention = LibraryCallingConvention)]
		extern static public HDC GetDC(HWND hWnd);

		[DllImport(Library, CallingConvention = LibraryCallingConvention)]
		extern static public int ChoosePixelFormat(HDC hdc, PIXELFORMATDESCRIPTOR* pfd);

#if false
		static HINSTANCE hInstance = 0;

		static HWND CreateOpenGLWindow(int width, int height, byte type, DWORD flags)
		{
			int         pf;
			HDC         hDC;
			HWND        hWnd;
			WNDCLASS    wc;
			PIXELFORMATDESCRIPTOR pfd;

			if (hInstance == 0)
			{
				hInstance        = GetModuleHandleA(null);
				wc.style         = CS_OWNDC;
				wc.lpfnWndProc   = DefWindowProcA;
				wc.cbClsExtra    = 0;
				wc.cbWndExtra    = 0;
				wc.hInstance     = hInstance;
				wc.hIcon         = LoadIconA(null, (char*)32517);
				wc.hCursor       = LoadCursorA(null, (char*)0);
				wc.hbrBackground = (HBRUSH)0;
				wc.lpszMenuName  = null;
				wc.lpszClassName = "PSPGE";

				if (!RegisterClassA(&wc)) throw(new Exception("RegisterClass() failed:  Cannot register window class."));
			}

			int dwStyle = WS_OVERLAPPEDWINDOW | WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
			RECT rc;
			rc.top = rc.left = 0;
			rc.right = width;
			rc.bottom = height;
			AdjustWindowRect(&rc, dwStyle, FALSE);
			hWnd = CreateWindowA("PSPGE", null, dwStyle, rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top, null, null, hInstance, null);
			if (hWnd is null) throw(new Exception("CreateWindow() failed:  Cannot create a window. : "));

			hDC = GetDC(hWnd);

			// http://msdn.microsoft.com/en-us/library/ms970745.aspx
			// http://www.gamedev.net/reference/articles/article540.asp
			pfd.nSize        = (WORD)sizeof(PIXELFORMATDESCRIPTOR);
			pfd.nVersion     = (WORD)1;
			//pfd.dwFlags      = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | flags;
			pfd.dwFlags      = (DWORD)(0x00000004 | 0x00000020 | flags);
				
			//pfd.iLayerType   = PFD_MAIN_PLANE;
			pfd.iLayerType   = 0;
			pfd.iPixelType   = type; // PFD_TYPE_RGBA
			pfd.cColorBits   = 24;
			//pfd.cDepthBits   = 8;
			pfd.cDepthBits   = 16;
			pfd.cStencilBits = 8;

			pf = ChoosePixelFormat(hDC, &pfd);

			if (pf == 0) throw(new Exception("ChoosePixelFormat() failed:  Cannot find a suitable pixel format."));

			if (SetPixelFormat(hDC, pf, &pfd) == 0) throw(new Exception("SetPixelFormat() failed:  Cannot set format specified."));

			DescribePixelFormat(hDC, pf, sizeof(PIXELFORMATDESCRIPTOR), &pfd);
			ReleaseDC(hDC, hWnd);

			return hWnd;
		}
#endif
	}

	public class GraphicsContext : IGraphicsContext, IGraphicsContextInternal
	{
		bool VSync;

		public GraphicsContext(GraphicsMode GraphicsMode, IWindowInfo IWindowInfo)
		{
			//throw new NotImplementedException();
		}

		void IGraphicsContext.MakeCurrent(IWindowInfo IWindowInfo)
		{
			//IGL.wglMakeCurrent();
			//throw new NotImplementedException();
		}

		bool IGraphicsContext.VSync
		{
			get
			{
				return VSync;
			}
			set
			{
				VSync = value;
				GL.VSync(value);
			}
		}

		void IGraphicsContext.SwapBuffers()
		{
			//throw new NotImplementedException();
		}

		void IGraphicsContextInternal.LoadAll()
		{
		}
	}

	public struct ColorFormat
	{
		public int Alpha;
		public int Blue;
		public int Green;
		public int Red;

		public int BitsPerPixel;
		public bool IsIndexed;
	}

	public class GraphicsMode
	{
		public GraphicsMode()
		{
		}

		public GraphicsMode(ColorFormat ColorFormat, int Depth, int Stencil, int AntiAlias, ColorFormat AccumulatorFormat, int Buffers, bool Stereo)
		{
			this.ColorFormat = ColorFormat;
			this.Depth = Depth;
			this.Stencil = Stencil;
			this.AntiAlias = AntiAlias;
			this.AccumulatorFormat = AccumulatorFormat;
			this.Buffers = Buffers;
			this.Stereo = Stereo;
		}
		public int AntiAlias;
		public ColorFormat AccumulatorFormat { get; private set; }
		public int Buffers { get; private set; }
		public ColorFormat ColorFormat { get; private set; }
		public int Depth { get; private set; }
		//public IntPtr? Index { get; private set; }
		public int Samples { get; private set; }
		public int Stencil { get; private set; }
		public bool Stereo { get; private set; }
		/*
		gm.ColorFormat,
		gm.Depth,
		8, //gm.Stencil,
		gm.Samples, // 4 // anti-alias
		gm.AccumulatorFormat,
		gm.Buffers,
		gm.Stereo
		*/

		static public GraphicsMode Default
		{
			get
			{
				return new GraphicsMode();
			}
		}
	}
}
