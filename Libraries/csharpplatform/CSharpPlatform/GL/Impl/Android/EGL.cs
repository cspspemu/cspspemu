using System.Runtime.InteropServices;
using EGLBoolean = System.Int32;
using EGLenum = System.Int32;
using EGLint = System.Int32;
using EGLNativeDisplayType = System.Int32;
using EGLConfig = System.Int32;
using EGLContext = System.Int32;
using EGLDisplay = System.Int32;
using EGLSurface = System.Int32;
using EGLClientBuffer = System.Int32;
using EGLNativePixmapType = System.IntPtr;
using EGLNativeWindowType = System.IntPtr;


namespace CSharpPlatform.GL.Impl.Android
{
    public unsafe class EGL
    {
        const string Library = "libEGL.so";

        public const int EGL_VERSION_1_0 = 1;
        public const int EGL_VERSION_1_1 = 1;
        public const int EGL_VERSION_1_2 = 1;
        public const int EGL_VERSION_1_3 = 1;
        public const int EGL_VERSION_1_4 = 1;
        public const int EGL_FALSE = 0;
        public const int EGL_TRUE = 1;
        public const int EGL_DEFAULT_DISPLAY = 0;
        public const int EGL_NO_CONTEXT = 0;
        public const int EGL_NO_DISPLAY = 0;
        public const int EGL_NO_SURFACE = 0;
        public const int EGL_DONT_CARE = unchecked(-1);
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
        public const int EGL_CONTEXT_LOST = 0x300E;
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
        public const int EGL_PRESERVED_RESOURCES = 0x3030;
        public const int EGL_SAMPLES = 0x3031;
        public const int EGL_SAMPLE_BUFFERS = 0x3032;
        public const int EGL_SURFACE_TYPE = 0x3033;
        public const int EGL_TRANSPARENT_TYPE = 0x3034;
        public const int EGL_TRANSPARENT_BLUE_VALUE = 0x3035;
        public const int EGL_TRANSPARENT_GREEN_VALUE = 0x3036;
        public const int EGL_TRANSPARENT_RED_VALUE = 0x3037;
        public const int EGL_NONE = 0x3038;
        public const int EGL_BIND_TO_TEXTURE_RGB = 0x3039;
        public const int EGL_BIND_TO_TEXTURE_RGBA = 0x303A;
        public const int EGL_MIN_SWAP_INTERVAL = 0x303B;
        public const int EGL_MAX_SWAP_INTERVAL = 0x303C;
        public const int EGL_LUMINANCE_SIZE = 0x303D;
        public const int EGL_ALPHA_MASK_SIZE = 0x303E;
        public const int EGL_COLOR_BUFFER_TYPE = 0x303F;
        public const int EGL_RENDERABLE_TYPE = 0x3040;
        public const int EGL_MATCH_NATIVE_PIXMAP = 0x3041;
        public const int EGL_CONFORMANT = 0x3042;
        public const int EGL_SLOW_CONFIG = 0x3050;
        public const int EGL_NON_CONFORMANT_CONFIG = 0x3051;
        public const int EGL_TRANSPARENT_RGB = 0x3052;
        public const int EGL_RGB_BUFFER = 0x308E;
        public const int EGL_LUMINANCE_BUFFER = 0x308F;
        public const int EGL_NO_TEXTURE = 0x305C;
        public const int EGL_TEXTURE_RGB = 0x305D;
        public const int EGL_TEXTURE_RGBA = 0x305E;
        public const int EGL_TEXTURE_2D = 0x305F;
        public const int EGL_PBUFFER_BIT = 0x0001;
        public const int EGL_PIXMAP_BIT = 0x0002;
        public const int EGL_WINDOW_BIT = 0x0004;
        public const int EGL_VG_COLORSPACE_LINEAR_BIT = 0x0020;
        public const int EGL_VG_ALPHA_FORMAT_PRE_BIT = 0x0040;
        public const int EGL_MULTISAMPLE_RESOLVE_BOX_BIT = 0x0200;
        public const int EGL_SWAP_BEHAVIOR_PRESERVED_BIT = 0x0400;
        public const int EGL_OPENGL_ES_BIT = 0x0001;
        public const int EGL_OPENVG_BIT = 0x0002;
        public const int EGL_OPENGL_ES2_BIT = 0x0004;
        public const int EGL_OPENGL_BIT = 0x0008;
        public const int EGL_VENDOR = 0x3053;
        public const int EGL_VERSION = 0x3054;
        public const int EGL_EXTENSIONS = 0x3055;
        public const int EGL_CLIENT_APIS = 0x308D;
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
        public const int EGL_BACK_BUFFER = 0x3084;
        public const int EGL_SINGLE_BUFFER = 0x3085;
        public const int EGL_VG_COLORSPACE_sRGB = 0x3089;
        public const int EGL_VG_COLORSPACE_LINEAR = 0x308A;
        public const int EGL_VG_ALPHA_FORMAT_NONPRE = 0x308B;
        public const int EGL_VG_ALPHA_FORMAT_PRE = 0x308C;
        public const int EGL_DISPLAY_SCALING = 10000;
        public const int EGL_UNKNOWN = unchecked(-1);
        public const int EGL_BUFFER_PRESERVED = 0x3094;
        public const int EGL_BUFFER_DESTROYED = 0x3095;
        public const int EGL_OPENVG_IMAGE = 0x3096;
        public const int EGL_CONTEXT_CLIENT_TYPE = 0x3097;
        public const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
        public const int EGL_MULTISAMPLE_RESOLVE_DEFAULT = 0x309A;
        public const int EGL_MULTISAMPLE_RESOLVE_BOX = 0x309B;
        public const int EGL_OPENGL_ES_API = 0x30A0;
        public const int EGL_OPENVG_API = 0x30A1;
        public const int EGL_OPENGL_API = 0x30A2;
        public const int EGL_DRAW = 0x3059;
        public const int EGL_READ = 0x305A;
        public const int EGL_CORE_NATIVE_ENGINE = 0x305B;
        public const int EGL_COLORSPACE = EGL_VG_COLORSPACE;
        public const int EGL_ALPHA_FORMAT = EGL_VG_ALPHA_FORMAT;
        public const int EGL_COLORSPACE_sRGB = EGL_VG_COLORSPACE_sRGB;
        public const int EGL_COLORSPACE_LINEAR = EGL_VG_COLORSPACE_LINEAR;
        public const int EGL_ALPHA_FORMAT_NONPRE = EGL_VG_ALPHA_FORMAT_NONPRE;
        public const int EGL_ALPHA_FORMAT_PRE = EGL_VG_ALPHA_FORMAT_PRE;

        [DllImport(Library)]
        public static extern int eglGetError();

        [DllImport(Library)]
        public static extern int eglGetDisplay(int display_id);

        [DllImport(Library)]
        public static extern int eglInitialize(int dpy, int* major, int* minor);

        [DllImport(Library)]
        public static extern int eglTerminate(int dpy);

        [DllImport(Library)]
        public static extern byte* eglQueryString(int dpy, int name);

        [DllImport(Library)]
        public static extern int eglGetConfigs(int dpy, int* configs, int config_size,
            int* num_config);

        [DllImport(Library)]
        public static extern int eglChooseConfig(int dpy, int* attrib_list, int* configs,
            int config_size, int* num_config);

        [DllImport(Library)]
        public static extern int eglGetConfigAttrib(int dpy, int config, int attribute,
            int* value);

        [DllImport(Library)]
        public static extern int eglCreateWindowSurface(int dpy, int config,
            EGLNativeWindowType win, int* attrib_list);

        [DllImport(Library)]
        public static extern int eglCreatePbufferSurface(int dpy, int config, int* attrib_list);

        [DllImport(Library)]
        public static extern int eglCreatePixmapSurface(int dpy, int config,
            EGLNativePixmapType pixmap, int* attrib_list);

        [DllImport(Library)]
        public static extern int eglDestroySurface(int dpy, int surface);

        [DllImport(Library)]
        public static extern int eglQuerySurface(int dpy, int surface, int attribute,
            int* value);

        [DllImport(Library)]
        public static extern int eglBindAPI(int api);

        [DllImport(Library)]
        public static extern int eglQueryAPI();

        [DllImport(Library)]
        public static extern int eglWaitClient();

        [DllImport(Library)]
        public static extern int eglReleaseThread();

        [DllImport(Library)]
        public static extern int eglCreatePbufferFromClientBuffer(int dpy, int buftype,
            int buffer, int config, int* attrib_list);

        [DllImport(Library)]
        public static extern int eglSurfaceAttrib(int dpy, int surface, int attribute,
            int value);

        [DllImport(Library)]
        public static extern int eglBindTexImage(int dpy, int surface, int buffer);

        [DllImport(Library)]
        public static extern int eglReleaseTexImage(int dpy, int surface, int buffer);

        [DllImport(Library)]
        public static extern int eglSwapInterval(int dpy, int interval);

        [DllImport(Library)]
        public static extern int eglCreateContext(int dpy, int config, int share_context,
            int* attrib_list);

        [DllImport(Library)]
        public static extern int eglDestroyContext(int dpy, int ctx);

        [DllImport(Library)]
        public static extern int
            eglMakeCurrent(int dpy, int draw, int read, int ctx);

        [DllImport(Library)]
        public static extern int eglGetCurrentContext();

        [DllImport(Library)]
        public static extern int eglGetCurrentSurface(int readdraw);

        [DllImport(Library)]
        public static extern int eglGetCurrentDisplay();

        [DllImport(Library)]
        public static extern int
            eglQueryContext(int dpy, int ctx, int attribute, int* value);

        [DllImport(Library)]
        public static extern int eglWaitGL();

        [DllImport(Library)]
        public static extern int eglWaitNative(int engine);

        [DllImport(Library)]
        public static extern int eglSwapBuffers(int dpy, int surface);

        [DllImport(Library)]
        public static extern int eglCopyBuffers(int dpy, int surface, EGLNativePixmapType target);

        [DllImport(Library)]
        public static extern EGLNativePixmapType eglGetProcAddress(string procname);

        //typedef void (*__eglMustCastToProperFunctionPointerType)(void);
    }
}