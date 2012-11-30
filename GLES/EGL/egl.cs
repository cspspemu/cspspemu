using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using EGLBoolean = System.Boolean;
using EGLenum = System.Int32;
using EGLint = System.Int32;

using EGLConfig = System.IntPtr;
using EGLContext = System.IntPtr;
using EGLDisplay = System.IntPtr;
using EGLSurface = System.IntPtr;
using EGLClientBuffer = System.IntPtr;

using EGLNativeDisplayType = System.IntPtr;
using EGLNativeWindowType = System.IntPtr;
using EGLNativePixmapType = System.IntPtr;

namespace GLES
{
	unsafe public partial class GL
	{
		/* -*- mode: c; tab-width: 8; -*- */
		/* vi: set sw=4 ts=8: */
		/* Reference version of egl.h for EGL 1.4.
		 * $Revision: 9356 $ on $Date: 2009-10-21 02:52:25 -0700 (Wed, 21 Oct 2009) $
		 */

		/*
		** Copyright (c) 2007-2009 The Khronos Group Inc.
		**
		** Permission is hereby granted, free of charge, to any person obtaining a
		** copy of this software and/or associated documentation files (the
		** "Materials"), to deal in the Materials without restriction, including
		** without limitation the rights to use, copy, modify, merge, publish,
		** distribute, sublicense, and/or sell copies of the Materials, and to
		** permit persons to whom the Materials are furnished to do so, subject to
		** the following conditions:
		**
		** The above copyright notice and this permission notice shall be included
		** in all copies or substantial portions of the Materials.
		**
		** THE MATERIALS ARE PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
		** EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
		** MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
		** IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
		** CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
		** TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
		** MATERIALS OR THE USE OR OTHER DEALINGS IN THE MATERIALS.
		*/

		/* All platform-dependent types and macro boilerplate (such as EGLAPI
		 * and EGLAPIENTRY) should go in eglplatform.h.
		 */
		//#include <EGL/eglplatform.h>

		/* EGL Types */
		/* EGLint is defined in eglplatform.h */
		//typedef unsigned int EGLBoolean;
		//typedef unsigned int EGLenum;
		//typedef void *EGLConfig;
		//typedef void *EGLContext;
		//typedef void *EGLDisplay;
		//typedef void *EGLSurface;
		//typedef void *EGLClientBuffer;

		/* EGL Versioning */
		public const int EGL_VERSION_1_0 = 1;
		public const int EGL_VERSION_1_1 = 1;
		public const int EGL_VERSION_1_2 = 1;
		public const int EGL_VERSION_1_3 = 1;
		public const int EGL_VERSION_1_4 = 1;

		/* EGL Enumerants. Bitmasks and other exceptional cases aside, most
		 * enums are assigned unique values starting at 0x3000.
		 */

		/* EGL aliases */
		public const bool EGL_FALSE = false;
		public const bool EGL_TRUE = true;

		/* Out-of-band handle values */
		public static EGLNativeDisplayType EGL_DEFAULT_DISPLAY = ((EGLNativeDisplayType)0);
		public static EGLContext EGL_NO_CONTEXT = ((EGLContext)0);
		public static EGLDisplay EGL_NO_DISPLAY = ((EGLDisplay)0);
		public static EGLSurface EGL_NO_SURFACE = ((EGLSurface)0);

		/* Out-of-band attribute value */
		public const int EGL_DONT_CARE = ((EGLint)(-1));

		/* Errors / GetError return values */
		public const int EGL_SUCCESS = 0x3000;
		public const int EGL_NOT_INITIALIZED = 0x3001;
		public const int EGL_BAD_ACCESS = 0x3002;
		public const int EGL_BAD_ALLOC = 0x3003;
		public const int EGL_BAD_ATTRIBUTE = 0x3004;
		public const int EGL_BAD_CONFIG = 0x3005;
		public const int EGL_BAD_CONTEXT = 0x3006;
		public const int EGL_BAD_CURRENT_SURFACE = 0x3007;
		public const int EGL_BAD_DISPLAY = 0x3008;
		public const int EGL_BAD_MATCH = 0x3009;
		public const int EGL_BAD_NATIVE_PIXMAP = 0x300A;
		public const int EGL_BAD_NATIVE_WINDOW = 0x300B;
		public const int EGL_BAD_PARAMETER = 0x300C;
		public const int EGL_BAD_SURFACE = 0x300D;
		public const int EGL_CONTEXT_LOST = 0x300E;	/* EGL 1.1 - IMG_power_management */

		static public string eglGetErrorString(EGLint error)
		{
			switch (error)
			{
				case EGL_SUCCESS: return "EGL_SUCCESS";
				case EGL_NOT_INITIALIZED: return "EGL_NOT_INITIALIZED";
				case EGL_BAD_ACCESS: return "EGL_BAD_ACCESS";
				case EGL_BAD_ALLOC: return "EGL_BAD_ALLOC";
				case EGL_BAD_ATTRIBUTE: return "EGL_BAD_ATTRIBUTE";
				case EGL_BAD_CONFIG: return "EGL_BAD_CONFIG";
				case EGL_BAD_CONTEXT: return "EGL_BAD_CONTEXT";
				case EGL_BAD_CURRENT_SURFACE: return "EGL_BAD_CURRENT_SURFACE";
				case EGL_BAD_DISPLAY: return "EGL_BAD_DISPLAY";
				case EGL_BAD_MATCH: return "EGL_BAD_MATCH";
				case EGL_BAD_NATIVE_PIXMAP: return "EGL_BAD_NATIVE_PIXMAP";
				case EGL_BAD_NATIVE_WINDOW: return "EGL_BAD_NATIVE_WINDOW";
				case EGL_BAD_PARAMETER: return "EGL_BAD_PARAMETER";
				case EGL_BAD_SURFACE: return "EGL_BAD_SURFACE";
				case EGL_CONTEXT_LOST: return "EGL_CONTEXT_LOST";
				default: return String.Format("EGL_UNKNOWN_{0:X4}", error);
			}
		}

		static public string eglGetErrorString()
		{
			return eglGetErrorString(eglGetError());
		}

		/* Reserved 0x300F-0x301F for additional errors */

		/* Config attributes */
		public const int EGL_BUFFER_SIZE = 0x3020;
		public const int EGL_ALPHA_SIZE = 0x3021;
		public const int EGL_BLUE_SIZE = 0x3022;
		public const int EGL_GREEN_SIZE = 0x3023;
		public const int EGL_RED_SIZE = 0x3024;
		public const int EGL_DEPTH_SIZE = 0x3025;
		public const int EGL_STENCIL_SIZE = 0x3026;
		public const int EGL_CONFIG_CAVEAT = 0x3027;
		public const int EGL_CONFIG_ID = 0x3028;
		public const int EGL_LEVEL = 0x3029;
		public const int EGL_MAX_PBUFFER_HEIGHT = 0x302A;
		public const int EGL_MAX_PBUFFER_PIXELS = 0x302B;
		public const int EGL_MAX_PBUFFER_WIDTH = 0x302C;
		public const int EGL_NATIVE_RENDERABLE = 0x302D;
		public const int EGL_NATIVE_VISUAL_ID = 0x302E;
		public const int EGL_NATIVE_VISUAL_TYPE = 0x302F;
		public const int EGL_SAMPLES = 0x3031;
		public const int EGL_SAMPLE_BUFFERS = 0x3032;
		public const int EGL_SURFACE_TYPE = 0x3033;
		public const int EGL_TRANSPARENT_TYPE = 0x3034;
		public const int EGL_TRANSPARENT_BLUE_VALUE = 0x3035;
		public const int EGL_TRANSPARENT_GREEN_VALUE = 0x3036;
		public const int EGL_TRANSPARENT_RED_VALUE = 0x3037;
		public const int EGL_NONE = 0x3038;	/* Attrib list terminator */
		public const int EGL_BIND_TO_TEXTURE_RGB = 0x3039;
		public const int EGL_BIND_TO_TEXTURE_RGBA = 0x303A;
		public const int EGL_MIN_SWAP_INTERVAL = 0x303B;
		public const int EGL_MAX_SWAP_INTERVAL = 0x303C;
		public const int EGL_LUMINANCE_SIZE = 0x303D;
		public const int EGL_ALPHA_MASK_SIZE = 0x303E;
		public const int EGL_COLOR_BUFFER_TYPE = 0x303F;
		public const int EGL_RENDERABLE_TYPE = 0x3040;
		public const int EGL_MATCH_NATIVE_PIXMAP = 0x3041;	/* Pseudo-attribute (not queryable) */
		public const int EGL_CONFORMANT = 0x3042;

		/* Reserved 0x3041-0x304F for additional config attributes */

		/* Config attribute values */
		public const int EGL_SLOW_CONFIG = 0x3050;	/* EGL_CONFIG_CAVEAT value */
		public const int EGL_NON_CONFORMANT_CONFIG = 0x3051;	/* EGL_CONFIG_CAVEAT value */
		public const int EGL_TRANSPARENT_RGB = 0x3052;	/* EGL_TRANSPARENT_TYPE value */
		public const int EGL_RGB_BUFFER = 0x308E;	/* EGL_COLOR_BUFFER_TYPE value */
		public const int EGL_LUMINANCE_BUFFER = 0x308F;	/* EGL_COLOR_BUFFER_TYPE value */

		/* More config attribute values, for EGL_TEXTURE_FORMAT */
		public const int EGL_NO_TEXTURE = 0x305C;
		public const int EGL_TEXTURE_RGB = 0x305D;
		public const int EGL_TEXTURE_RGBA = 0x305E;
		public const int EGL_TEXTURE_2D = 0x305F;

		/* Config attribute mask bits */
		public const int EGL_PBUFFER_BIT = 0x0001;	/* EGL_SURFACE_TYPE mask bits */
		public const int EGL_PIXMAP_BIT = 0x0002;	/* EGL_SURFACE_TYPE mask bits */
		public const int EGL_WINDOW_BIT = 0x0004;	/* EGL_SURFACE_TYPE mask bits */
		public const int EGL_VG_COLORSPACE_LINEAR_BIT = 0x0020;	/* EGL_SURFACE_TYPE mask bits */
		public const int EGL_VG_ALPHA_FORMAT_PRE_BIT = 0x0040;	/* EGL_SURFACE_TYPE mask bits */
		public const int EGL_MULTISAMPLE_RESOLVE_BOX_BIT = 0x0200;	/* EGL_SURFACE_TYPE mask bits */
		public const int EGL_SWAP_BEHAVIOR_PRESERVED_BIT = 0x0400;	/* EGL_SURFACE_TYPE mask bits */

		public const int EGL_OPENGL_ES_BIT = 0x0001;	/* EGL_RENDERABLE_TYPE mask bits */
		public const int EGL_OPENVG_BIT = 0x0002;	/* EGL_RENDERABLE_TYPE mask bits */
		public const int EGL_OPENGL_ES2_BIT = 0x0004;	/* EGL_RENDERABLE_TYPE mask bits */
		public const int EGL_OPENGL_BIT = 0x0008;	/* EGL_RENDERABLE_TYPE mask bits */

		/* QueryString targets */
		public const int EGL_VENDOR = 0x3053;
		public const int EGL_VERSION = 0x3054;
		public const int EGL_EXTENSIONS = 0x3055;
		public const int EGL_CLIENT_APIS = 0x308D;

		/* QuerySurface / SurfaceAttrib / CreatePbufferSurface targets */
		public const int EGL_HEIGHT = 0x3056;
		public const int EGL_WIDTH = 0x3057;
		public const int EGL_LARGEST_PBUFFER = 0x3058;
		public const int EGL_TEXTURE_FORMAT = 0x3080;
		public const int EGL_TEXTURE_TARGET = 0x3081;
		public const int EGL_MIPMAP_TEXTURE = 0x3082;
		public const int EGL_MIPMAP_LEVEL = 0x3083;
		public const int EGL_RENDER_BUFFER = 0x3086;
		public const int EGL_VG_COLORSPACE = 0x3087;
		public const int EGL_VG_ALPHA_FORMAT = 0x3088;
		public const int EGL_HORIZONTAL_RESOLUTION = 0x3090;
		public const int EGL_VERTICAL_RESOLUTION = 0x3091;
		public const int EGL_PIXEL_ASPECT_RATIO = 0x3092;
		public const int EGL_SWAP_BEHAVIOR = 0x3093;
		public const int EGL_MULTISAMPLE_RESOLVE = 0x3099;

		/* EGL_RENDER_BUFFER values / BindTexImage / ReleaseTexImage buffer targets */
		public const int EGL_BACK_BUFFER = 0x3084;
		public const int EGL_SINGLE_BUFFER = 0x3085;

		/* OpenVG color spaces */
		public const int EGL_VG_COLORSPACE_sRGB = 0x3089;	/* EGL_VG_COLORSPACE value */
		public const int EGL_VG_COLORSPACE_LINEAR = 0x308A;	/* EGL_VG_COLORSPACE value */

		/* OpenVG alpha formats */
		public const int EGL_VG_ALPHA_FORMAT_NONPRE = 0x308B;	/* EGL_ALPHA_FORMAT value */
		public const int EGL_VG_ALPHA_FORMAT_PRE = 0x308C;	/* EGL_ALPHA_FORMAT value */

		/* Constant scale factor by which fractional display resolutions &
		 * aspect ratio are scaled when queried as integer values.
		 */
		public const int EGL_DISPLAY_SCALING = 10000;

		/* Unknown display resolution/aspect ratio */
		public const int EGL_UNKNOWN = ((EGLint)(-1));

		/* Back buffer swap behaviors */
		public const int EGL_BUFFER_PRESERVED = 0x3094;	/* EGL_SWAP_BEHAVIOR value */
		public const int EGL_BUFFER_DESTROYED = 0x3095;	/* EGL_SWAP_BEHAVIOR value */

		/* CreatePbufferFromClientBuffer buffer types */
		public const int EGL_OPENVG_IMAGE = 0x3096;

		/* QueryContext targets */
		public const int EGL_CONTEXT_CLIENT_TYPE = 0x3097;

		/* CreateContext attributes */
		public const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;

		/* Multisample resolution behaviors */
		public const int EGL_MULTISAMPLE_RESOLVE_DEFAULT = 0x309A;	/* EGL_MULTISAMPLE_RESOLVE value */
		public const int EGL_MULTISAMPLE_RESOLVE_BOX = 0x309B;	/* EGL_MULTISAMPLE_RESOLVE value */

		/* BindAPI/QueryAPI targets */
		public const int EGL_OPENGL_ES_API = 0x30A0;
		public const int EGL_OPENVG_API = 0x30A1;
		public const int EGL_OPENGL_API = 0x30A2;
		
		/* GetCurrentSurface targets */
		public const int EGL_DRAW = 0x3059;
		public const int EGL_READ = 0x305A;

		/* WaitNative engines */
		public const int EGL_CORE_NATIVE_ENGINE = 0x305B;

		/* EGL 1.2 tokens renamed for consistency in EGL 1.3 */
		public const int EGL_COLORSPACE = EGL_VG_COLORSPACE;
		public const int EGL_ALPHA_FORMAT = EGL_VG_ALPHA_FORMAT;
		public const int EGL_COLORSPACE_sRGB = EGL_VG_COLORSPACE_sRGB;
		public const int EGL_COLORSPACE_LINEAR = EGL_VG_COLORSPACE_LINEAR;
		public const int EGL_ALPHA_FORMAT_NONPRE = EGL_VG_ALPHA_FORMAT_NONPRE;
		public const int EGL_ALPHA_FORMAT_PRE = EGL_VG_ALPHA_FORMAT_PRE;

		/* EGL extensions must request enum blocks from the Khronos
		 * API Registrar, who maintains the enumerant registry. Submit
		 * a bug in Khronos Bugzilla against task "Registry".
		 */



		/* EGL Functions */

		[DllImport("libEGL")] static public extern EGLint eglGetError();
		[DllImport("libEGL")] static public extern EGLDisplay eglGetDisplay(EGLNativeDisplayType display_id);
		[DllImport("libEGL")] static public extern EGLBoolean eglInitialize(EGLDisplay dpy, EGLint* major, EGLint* minor);
		[DllImport("libEGL")] static public extern EGLBoolean eglTerminate(EGLDisplay dpy);
		[DllImport("libEGL")] static public extern string eglQueryString(EGLDisplay dpy, EGLint name);
		[DllImport("libEGL")] static public extern EGLBoolean eglGetConfigs(EGLDisplay dpy, EGLConfig* configs, EGLint config_size, EGLint* num_config);
		[DllImport("libEGL")] static public extern EGLBoolean eglChooseConfig(EGLDisplay dpy, EGLint* attrib_list, EGLConfig* configs, EGLint config_size, EGLint* num_config);
		[DllImport("libEGL")] static public extern EGLBoolean eglGetConfigAttrib(EGLDisplay dpy, EGLConfig config, EGLint attribute, EGLint* value);
		[DllImport("libEGL")] static public extern EGLSurface eglCreateWindowSurface(EGLDisplay dpy, EGLConfig config, EGLNativeWindowType win, EGLint* attrib_list);
		[DllImport("libEGL")] static public extern EGLSurface eglCreatePbufferSurface(EGLDisplay dpy, EGLConfig config, EGLint* attrib_list);
		[DllImport("libEGL")] static public extern EGLSurface eglCreatePixmapSurface(EGLDisplay dpy, EGLConfig config, EGLNativePixmapType pixmap, EGLint* attrib_list);
		[DllImport("libEGL")] static public extern EGLBoolean eglDestroySurface(EGLDisplay dpy, EGLSurface surface);
		[DllImport("libEGL")] static public extern EGLBoolean eglQuerySurface(EGLDisplay dpy, EGLSurface surface, EGLint attribute, EGLint* value);
		[DllImport("libEGL")] static public extern EGLBoolean eglBindAPI(EGLenum api);
		[DllImport("libEGL")] static public extern EGLenum eglQueryAPI();
		[DllImport("libEGL")] static public extern EGLBoolean eglWaitClient();
		[DllImport("libEGL")] static public extern EGLBoolean eglReleaseThread();
		[DllImport("libEGL")] static public extern EGLSurface eglCreatePbufferFromClientBuffer(EGLDisplay dpy, EGLenum buftype, EGLClientBuffer buffer, EGLConfig config, EGLint* attrib_list);
		[DllImport("libEGL")] static public extern EGLBoolean eglSurfaceAttrib(EGLDisplay dpy, EGLSurface surface, EGLint attribute, EGLint value);
		[DllImport("libEGL")] static public extern EGLBoolean eglBindTexImage(EGLDisplay dpy, EGLSurface surface, EGLint buffer);
		[DllImport("libEGL")] static public extern EGLBoolean eglReleaseTexImage(EGLDisplay dpy, EGLSurface surface, EGLint buffer);
		[DllImport("libEGL")] static public extern EGLBoolean eglSwapInterval(EGLDisplay dpy, EGLint interval);
		[DllImport("libEGL")] static public extern EGLContext eglCreateContext(EGLDisplay dpy, EGLConfig config, EGLContext share_context, EGLint* attrib_list);
		[DllImport("libEGL")] static public extern EGLBoolean eglDestroyContext(EGLDisplay dpy, EGLContext ctx);
		[DllImport("libEGL")] static public extern EGLBoolean eglMakeCurrent(EGLDisplay dpy, EGLSurface draw, EGLSurface read, EGLContext ctx);
		[DllImport("libEGL")] static public extern EGLContext eglGetCurrentContext();
		[DllImport("libEGL")] static public extern EGLSurface eglGetCurrentSurface(EGLint readdraw);
		[DllImport("libEGL")] static public extern EGLDisplay eglGetCurrentDisplay();
		[DllImport("libEGL")] static public extern EGLBoolean eglQueryContext(EGLDisplay dpy, EGLContext ctx, EGLint attribute, EGLint* value);
		[DllImport("libEGL")] static public extern EGLBoolean eglWaitGL();
		[DllImport("libEGL")] static public extern EGLBoolean eglWaitNative(EGLint engine);
		[DllImport("libEGL")] static public extern EGLBoolean eglSwapBuffers(EGLDisplay dpy, EGLSurface surface);
		[DllImport("libEGL")] static public extern EGLBoolean eglCopyBuffers(EGLDisplay dpy, EGLSurface surface, EGLNativePixmapType target);

		/* This is a generic function pointer type, whose name indicates it must
		 * be cast to the proper type *and calling convention* before use.
		 */
		//typedef void (*__eglMustCastToProperFunctionPointerType)(void);

		/* Now, define eglGetProcAddress using the generic function ptr. type */
		//EGLAPI __eglMustCastToProperFunctionPointerType EGLAPIENTRY eglGetProcAddress(const char *procname);

		public static EGLint eglGetConfigAttrib(EGLDisplay dpy, EGLConfig config, EGLint attribute)
		{
			EGLint @out = 0;
			if (!eglGetConfigAttrib(dpy, config, attribute, &@out)) throw(new Exception("Can't get value"));
			return @out;
		}
	}
}
