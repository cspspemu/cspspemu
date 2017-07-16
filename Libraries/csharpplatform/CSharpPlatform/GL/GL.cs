using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using CSharpPlatform.Library;
using EGLBoolean = System.Boolean;
using EGLenum = System.Int32;
using EGLint = System.Int32;
using GLsizeiptr = System.UInt32;
using GLbitfield = System.UInt32;
using GLenum = System.Int32;
using GLboolean = System.Boolean;
using GLsizei = System.Int32;
using GLclampf = System.Single;
using GLfloat = System.Single;
using GLuint = System.UInt32;
using GLint = System.Int32;
using GLchar = System.Byte;
using GLubyte = System.Byte;
using GLintptr = System.IntPtr;
using EGLConfig = System.IntPtr;
using EGLContext = System.IntPtr;
using EGLDisplay = System.IntPtr;
using EGLSurface = System.IntPtr;
using EGLClientBuffer = System.IntPtr;
using EGLNativeDisplayType = System.IntPtr;
using EGLNativeWindowType = System.IntPtr;
using EGLNativePixmapType = System.IntPtr;

// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedReadonlyField

namespace CSharpPlatform.GL
{
    public unsafe class GL
    {
        internal static readonly object Lock = new object();

        internal const string DllWindows = "OpenGL32";
        internal const string DllLinux = "libGL.so.1";
        internal const string DllMac = "/System/Library/Frameworks/OpenGL.framework/OpenGL";
        internal const string DllAndroid = "libopengl.so.1";

        // "opengl32.dll",
        // "libGL.so.2,libGL.so.1,libGL.so",
        // "../Frameworks/OpenGL.framework/OpenGL, /Library/Frameworks/OpenGL.framework/OpenGL, /System/Library/Frameworks/OpenGL.framework/OpenGL"

        static GL()
        {
            DynamicLibraryFactory.MapLibraryToType<GL>(
                DynamicLibraryFactory.CreateForLibrary(DllWindows, DllLinux, DllMac, DllAndroid));
        }

        private static bool LoadedAll;

        internal static void LoadAllOnce()
        {
            if (!LoadedAll)
            {
                LoadedAll = true;
                DynamicLibraryFactory.MapLibraryToType<GL>(new DynamicLibraryGl());
            }
        }

        private static Dictionary<int, string> Constants;

        public static string GetConstantString(int Value)
        {
            lock (Lock)
            {
                if (Constants == null)
                {
                    Constants = new Dictionary<GLint, string>();
                    foreach (var Field in typeof(GL).GetFields(BindingFlags.Static | BindingFlags.Public))
                    {
                        if (Field.FieldType == typeof(int))
                        {
                            Constants[(int) Field.GetValue(null)] = Field.Name;
                        }
                    }
                }
                return Constants[Value];
            }
        }

        public const int GL_ES_VERSION_2_0 = 1;
        public const int GL_DEPTH_BUFFER_BIT = 0x00000100;
        public const int GL_STENCIL_BUFFER_BIT = 0x00000400;
        public const int GL_COLOR_BUFFER_BIT = 0x00004000;
        public const bool GL_FALSE = false;
        public const bool GL_TRUE = true;
        public const int GL_POINTS = 0x0000;
        public const int GL_LINES = 0x0001;
        public const int GL_LINE_LOOP = 0x0002;
        public const int GL_LINE_STRIP = 0x0003;
        public const int GL_TRIANGLES = 0x0004;
        public const int GL_TRIANGLE_STRIP = 0x0005;
        public const int GL_TRIANGLE_FAN = 0x0006;
        public const int GL_ZERO = 0;
        public const int GL_ONE = 1;
        public const int GL_SRC_COLOR = 0x0300;
        public const int GL_ONE_MINUS_SRC_COLOR = 0x0301;
        public const int GL_SRC_ALPHA = 0x0302;
        public const int GL_ONE_MINUS_SRC_ALPHA = 0x0303;
        public const int GL_DST_ALPHA = 0x0304;
        public const int GL_ONE_MINUS_DST_ALPHA = 0x0305;
        public const int GL_DST_COLOR = 0x0306;
        public const int GL_ONE_MINUS_DST_COLOR = 0x0307;
        public const int GL_SRC_ALPHA_SATURATE = 0x0308;
        public const int GL_FUNC_ADD = 0x8006;
        public const int GL_BLEND_EQUATION = 0x8009;
        public const int GL_BLEND_EQUATION_RGB = 0x8009;
        public const int GL_BLEND_EQUATION_ALPHA = 0x883D;
        public const int GL_FUNC_SUBTRACT = 0x800A;
        public const int GL_FUNC_REVERSE_SUBTRACT = 0x800B;
        public const int GL_BLEND_DST_RGB = 0x80C8;
        public const int GL_BLEND_SRC_RGB = 0x80C9;
        public const int GL_BLEND_DST_ALPHA = 0x80CA;
        public const int GL_BLEND_SRC_ALPHA = 0x80CB;
        public const int GL_CONSTANT_COLOR = 0x8001;
        public const int GL_ONE_MINUS_CONSTANT_COLOR = 0x8002;
        public const int GL_CONSTANT_ALPHA = 0x8003;
        public const int GL_ONE_MINUS_CONSTANT_ALPHA = 0x8004;
        public const int GL_BLEND_COLOR = 0x8005;
        public const int GL_ARRAY_BUFFER = 0x8892;
        public const int GL_ELEMENT_ARRAY_BUFFER = 0x8893;
        public const int GL_ARRAY_BUFFER_BINDING = 0x8894;
        public const int GL_ELEMENT_ARRAY_BUFFER_BINDING = 0x8895;
        public const int GL_STREAM_DRAW = 0x88E0;
        public const int GL_STATIC_DRAW = 0x88E4;
        public const int GL_DYNAMIC_DRAW = 0x88E8;
        public const int GL_BUFFER_SIZE = 0x8764;
        public const int GL_BUFFER_USAGE = 0x8765;
        public const int GL_CURRENT_VERTEX_ATTRIB = 0x8626;
        public const int GL_FRONT = 0x0404;
        public const int GL_BACK = 0x0405;
        public const int GL_FRONT_AND_BACK = 0x0408;
        public const int GL_TEXTURE_2D = 0x0DE1;
        public const int GL_CULL_FACE = 0x0B44;
        public const int GL_BLEND = 0x0BE2;
        public const int GL_DITHER = 0x0BD0;
        public const int GL_STENCIL_TEST = 0x0B90;
        public const int GL_DEPTH_TEST = 0x0B71;
        public const int GL_SCISSOR_TEST = 0x0C11;
        public const int GL_POLYGON_OFFSET_FILL = 0x8037;
        public const int GL_SAMPLE_ALPHA_TO_COVERAGE = 0x809E;
        public const int GL_SAMPLE_COVERAGE = 0x80A0;
        public const int GL_NO_ERROR = 0;
        public const int GL_INVALID_ENUM = 0x0500;
        public const int GL_INVALID_VALUE = 0x0501;
        public const int GL_INVALID_OPERATION = 0x0502;
        public const int GL_OUT_OF_MEMORY = 0x0505;
        public const int GL_CW = 0x0900;
        public const int GL_CCW = 0x0901;
        public const int GL_LINE_WIDTH = 0x0B21;
        public const int GL_ALIASED_POINT_SIZE_RANGE = 0x846D;
        public const int GL_ALIASED_LINE_WIDTH_RANGE = 0x846E;
        public const int GL_CULL_FACE_MODE = 0x0B45;
        public const int GL_FRONT_FACE = 0x0B46;
        public const int GL_DEPTH_RANGE = 0x0B70;
        public const int GL_DEPTH_WRITEMASK = 0x0B72;
        public const int GL_DEPTH_CLEAR_VALUE = 0x0B73;
        public const int GL_DEPTH_FUNC = 0x0B74;
        public const int GL_STENCIL_CLEAR_VALUE = 0x0B91;
        public const int GL_STENCIL_FUNC = 0x0B92;
        public const int GL_STENCIL_FAIL = 0x0B94;
        public const int GL_STENCIL_PASS_DEPTH_FAIL = 0x0B95;
        public const int GL_STENCIL_PASS_DEPTH_PASS = 0x0B96;
        public const int GL_STENCIL_REF = 0x0B97;
        public const int GL_STENCIL_VALUE_MASK = 0x0B93;
        public const int GL_STENCIL_WRITEMASK = 0x0B98;
        public const int GL_STENCIL_BACK_FUNC = 0x8800;
        public const int GL_STENCIL_BACK_FAIL = 0x8801;
        public const int GL_STENCIL_BACK_PASS_DEPTH_FAIL = 0x8802;
        public const int GL_STENCIL_BACK_PASS_DEPTH_PASS = 0x8803;
        public const int GL_STENCIL_BACK_REF = 0x8CA3;
        public const int GL_STENCIL_BACK_VALUE_MASK = 0x8CA4;
        public const int GL_STENCIL_BACK_WRITEMASK = 0x8CA5;
        public const int GL_VIEWPORT = 0x0BA2;
        public const int GL_SCISSOR_BOX = 0x0C10;
        public const int GL_COLOR_CLEAR_VALUE = 0x0C22;
        public const int GL_COLOR_WRITEMASK = 0x0C23;
        public const int GL_UNPACK_ALIGNMENT = 0x0CF5;
        public const int GL_PACK_ALIGNMENT = 0x0D05;
        public const int GL_MAX_TEXTURE_SIZE = 0x0D33;
        public const int GL_MAX_VIEWPORT_DIMS = 0x0D3A;
        public const int GL_SUBPIXEL_BITS = 0x0D50;
        public const int GL_RED_BITS = 0x0D52;
        public const int GL_GREEN_BITS = 0x0D53;
        public const int GL_BLUE_BITS = 0x0D54;
        public const int GL_ALPHA_BITS = 0x0D55;
        public const int GL_DEPTH_BITS = 0x0D56;
        public const int GL_STENCIL_BITS = 0x0D57;
        public const int GL_POLYGON_OFFSET_UNITS = 0x2A00;
        public const int GL_POLYGON_OFFSET_FACTOR = 0x8038;
        public const int GL_TEXTURE_BINDING_2D = 0x8069;
        public const int GL_SAMPLE_BUFFERS = 0x80A8;
        public const int GL_SAMPLES = 0x80A9;
        public const int GL_SAMPLE_COVERAGE_VALUE = 0x80AA;
        public const int GL_SAMPLE_COVERAGE_INVERT = 0x80AB;
        public const int GL_NUM_COMPRESSED_TEXTURE_FORMATS = 0x86A2;
        public const int GL_COMPRESSED_TEXTURE_FORMATS = 0x86A3;
        public const int GL_DONT_CARE = 0x1100;
        public const int GL_FASTEST = 0x1101;
        public const int GL_NICEST = 0x1102;
        public const int GL_GENERATE_MIPMAP_HINT = 0x8192;
        public const int GL_BYTE = 0x1400;
        public const int GL_UNSIGNED_BYTE = 0x1401;
        public const int GL_SHORT = 0x1402;
        public const int GL_UNSIGNED_SHORT = 0x1403;
        public const int GL_INT = 0x1404;
        public const int GL_UNSIGNED_INT = 0x1405;
        public const int GL_FLOAT = 0x1406;
        public const int GL_FIXED = 0x140C;
        public const int GL_DEPTH_COMPONENT = 0x1902;
        public const int GL_ALPHA = 0x1906;
        public const int GL_RGB = 0x1907;
        public const int GL_RGBA = 0x1908;
        public const int GL_LUMINANCE = 0x1909;
        public const int GL_LUMINANCE_ALPHA = 0x190A;
        public const int GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033;
        public const int GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034;
        public const int GL_UNSIGNED_SHORT_5_6_5 = 0x8363;
        public const int GL_FRAGMENT_SHADER = 0x8B30;
        public const int GL_VERTEX_SHADER = 0x8B31;
        public const int GL_MAX_VERTEX_ATTRIBS = 0x8869;
        public const int GL_MAX_VERTEX_UNIFORM_VECTORS = 0x8DFB;
        public const int GL_MAX_VARYING_VECTORS = 0x8DFC;
        public const int GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS = 0x8B4D;
        public const int GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8B4C;
        public const int GL_MAX_TEXTURE_IMAGE_UNITS = 0x8872;
        public const int GL_MAX_FRAGMENT_UNIFORM_VECTORS = 0x8DFD;
        public const int GL_SHADER_TYPE = 0x8B4F;
        public const int GL_DELETE_STATUS = 0x8B80;
        public const int GL_LINK_STATUS = 0x8B82;
        public const int GL_VALIDATE_STATUS = 0x8B83;
        public const int GL_ATTACHED_SHADERS = 0x8B85;
        public const int GL_ACTIVE_UNIFORMS = 0x8B86;
        public const int GL_ACTIVE_UNIFORM_MAX_LENGTH = 0x8B87;
        public const int GL_ACTIVE_ATTRIBUTES = 0x8B89;
        public const int GL_ACTIVE_ATTRIBUTE_MAX_LENGTH = 0x8B8A;
        public const int GL_SHADING_LANGUAGE_VERSION = 0x8B8C;
        public const int GL_CURRENT_PROGRAM = 0x8B8D;
        public const int GL_NEVER = 0x0200;
        public const int GL_LESS = 0x0201;
        public const int GL_EQUAL = 0x0202;
        public const int GL_LEQUAL = 0x0203;
        public const int GL_GREATER = 0x0204;
        public const int GL_NOTEQUAL = 0x0205;
        public const int GL_GEQUAL = 0x0206;
        public const int GL_ALWAYS = 0x0207;
        public const int GL_KEEP = 0x1E00;
        public const int GL_REPLACE = 0x1E01;
        public const int GL_INCR = 0x1E02;
        public const int GL_DECR = 0x1E03;
        public const int GL_INVERT = 0x150A;
        public const int GL_INCR_WRAP = 0x8507;
        public const int GL_DECR_WRAP = 0x8508;
        public const int GL_VENDOR = 0x1F00;
        public const int GL_RENDERER = 0x1F01;
        public const int GL_VERSION = 0x1F02;
        public const int GL_EXTENSIONS = 0x1F03;
        public const int GL_NEAREST = 0x2600;
        public const int GL_LINEAR = 0x2601;
        public const int GL_NEAREST_MIPMAP_NEAREST = 0x2700;
        public const int GL_LINEAR_MIPMAP_NEAREST = 0x2701;
        public const int GL_NEAREST_MIPMAP_LINEAR = 0x2702;
        public const int GL_LINEAR_MIPMAP_LINEAR = 0x2703;
        public const int GL_TEXTURE_MAG_FILTER = 0x2800;
        public const int GL_TEXTURE_MIN_FILTER = 0x2801;
        public const int GL_TEXTURE_WRAP_S = 0x2802;
        public const int GL_TEXTURE_WRAP_T = 0x2803;
        public const int GL_TEXTURE = 0x1702;
        public const int GL_TEXTURE_CUBE_MAP = 0x8513;
        public const int GL_TEXTURE_BINDING_CUBE_MAP = 0x8514;
        public const int GL_TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515;
        public const int GL_TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516;
        public const int GL_TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517;
        public const int GL_TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518;
        public const int GL_TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519;
        public const int GL_TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851A;
        public const int GL_MAX_CUBE_MAP_TEXTURE_SIZE = 0x851C;
        public const int GL_TEXTURE0 = 0x84C0;
        public const int GL_TEXTURE1 = 0x84C1;
        public const int GL_TEXTURE2 = 0x84C2;
        public const int GL_TEXTURE3 = 0x84C3;
        public const int GL_TEXTURE4 = 0x84C4;
        public const int GL_TEXTURE5 = 0x84C5;
        public const int GL_TEXTURE6 = 0x84C6;
        public const int GL_TEXTURE7 = 0x84C7;
        public const int GL_TEXTURE8 = 0x84C8;
        public const int GL_TEXTURE9 = 0x84C9;
        public const int GL_TEXTURE10 = 0x84CA;
        public const int GL_TEXTURE11 = 0x84CB;
        public const int GL_TEXTURE12 = 0x84CC;
        public const int GL_TEXTURE13 = 0x84CD;
        public const int GL_TEXTURE14 = 0x84CE;
        public const int GL_TEXTURE15 = 0x84CF;
        public const int GL_TEXTURE16 = 0x84D0;
        public const int GL_TEXTURE17 = 0x84D1;
        public const int GL_TEXTURE18 = 0x84D2;
        public const int GL_TEXTURE19 = 0x84D3;
        public const int GL_TEXTURE20 = 0x84D4;
        public const int GL_TEXTURE21 = 0x84D5;
        public const int GL_TEXTURE22 = 0x84D6;
        public const int GL_TEXTURE23 = 0x84D7;
        public const int GL_TEXTURE24 = 0x84D8;
        public const int GL_TEXTURE25 = 0x84D9;
        public const int GL_TEXTURE26 = 0x84DA;
        public const int GL_TEXTURE27 = 0x84DB;
        public const int GL_TEXTURE28 = 0x84DC;
        public const int GL_TEXTURE29 = 0x84DD;
        public const int GL_TEXTURE30 = 0x84DE;
        public const int GL_TEXTURE31 = 0x84DF;
        public const int GL_ACTIVE_TEXTURE = 0x84E0;
        public const int GL_REPEAT = 0x2901;
        public const int GL_CLAMP_TO_EDGE = 0x812F;
        public const int GL_MIRRORED_REPEAT = 0x8370;
        public const int GL_FLOAT_VEC2 = 0x8B50;
        public const int GL_FLOAT_VEC3 = 0x8B51;
        public const int GL_FLOAT_VEC4 = 0x8B52;
        public const int GL_INT_VEC2 = 0x8B53;
        public const int GL_INT_VEC3 = 0x8B54;
        public const int GL_INT_VEC4 = 0x8B55;
        public const int GL_BOOL = 0x8B56;
        public const int GL_BOOL_VEC2 = 0x8B57;
        public const int GL_BOOL_VEC3 = 0x8B58;
        public const int GL_BOOL_VEC4 = 0x8B59;
        public const int GL_FLOAT_MAT2 = 0x8B5A;
        public const int GL_FLOAT_MAT3 = 0x8B5B;
        public const int GL_FLOAT_MAT4 = 0x8B5C;
        public const int GL_SAMPLER_2D = 0x8B5E;
        public const int GL_SAMPLER_CUBE = 0x8B60;
        public const int GL_VERTEX_ATTRIB_ARRAY_ENABLED = 0x8622;
        public const int GL_VERTEX_ATTRIB_ARRAY_SIZE = 0x8623;
        public const int GL_VERTEX_ATTRIB_ARRAY_STRIDE = 0x8624;
        public const int GL_VERTEX_ATTRIB_ARRAY_TYPE = 0x8625;
        public const int GL_VERTEX_ATTRIB_ARRAY_NORMALIZED = 0x886A;
        public const int GL_VERTEX_ATTRIB_ARRAY_POINTER = 0x8645;
        public const int GL_VERTEX_ATTRIB_ARRAY_BUFFER_BINDING = 0x889F;
        public const int GL_IMPLEMENTATION_COLOR_READ_TYPE = 0x8B9A;
        public const int GL_IMPLEMENTATION_COLOR_READ_FORMAT = 0x8B9B;
        public const int GL_COMPILE_STATUS = 0x8B81;
        public const int GL_INFO_LOG_LENGTH = 0x8B84;
        public const int GL_SHADER_SOURCE_LENGTH = 0x8B88;
        public const int GL_SHADER_COMPILER = 0x8DFA;
        public const int GL_SHADER_BINARY_FORMATS = 0x8DF8;
        public const int GL_NUM_SHADER_BINARY_FORMATS = 0x8DF9;
        public const int GL_LOW_FLOAT = 0x8DF0;
        public const int GL_MEDIUM_FLOAT = 0x8DF1;
        public const int GL_HIGH_FLOAT = 0x8DF2;
        public const int GL_LOW_INT = 0x8DF3;
        public const int GL_MEDIUM_INT = 0x8DF4;
        public const int GL_HIGH_INT = 0x8DF5;
        public const int GL_FRAMEBUFFER = 0x8D40;
        public const int GL_RENDERBUFFER = 0x8D41;
        public const int GL_RGBA4 = 0x8056;
        public const int GL_RGB5_A1 = 0x8057;
        public const int GL_RGB565 = 0x8D62;
        public const int GL_DEPTH_COMPONENT16 = 0x81A5;
        public const int GL_STENCIL_INDEX = 0x1901;
        public const int GL_STENCIL_INDEX8 = 0x8D48;
        public const int GL_RENDERBUFFER_WIDTH = 0x8D42;
        public const int GL_RENDERBUFFER_HEIGHT = 0x8D43;
        public const int GL_RENDERBUFFER_INTERNAL_FORMAT = 0x8D44;
        public const int GL_RENDERBUFFER_RED_SIZE = 0x8D50;
        public const int GL_RENDERBUFFER_GREEN_SIZE = 0x8D51;
        public const int GL_RENDERBUFFER_BLUE_SIZE = 0x8D52;
        public const int GL_RENDERBUFFER_ALPHA_SIZE = 0x8D53;
        public const int GL_RENDERBUFFER_DEPTH_SIZE = 0x8D54;
        public const int GL_RENDERBUFFER_STENCIL_SIZE = 0x8D55;
        public const int GL_FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE = 0x8CD0;
        public const int GL_FRAMEBUFFER_ATTACHMENT_OBJECT_NAME = 0x8CD1;
        public const int GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL = 0x8CD2;
        public const int GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE = 0x8CD3;
        public const int GL_COLOR_ATTACHMENT0 = 0x8CE0;
        public const int GL_DEPTH_ATTACHMENT = 0x8D00;
        public const int GL_STENCIL_ATTACHMENT = 0x8D20;
        public const int GL_NONE = 0;
        public const int GL_FRAMEBUFFER_COMPLETE = 0x8CD5;
        public const int GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT = 0x8CD6;
        public const int GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8CD7;
        public const int GL_FRAMEBUFFER_INCOMPLETE_DIMENSIONS = 0x8CD9;
        public const int GL_FRAMEBUFFER_UNSUPPORTED = 0x8CDD;
        public const int GL_FRAMEBUFFER_BINDING = 0x8CA6;
        public const int GL_RENDERBUFFER_BINDING = 0x8CA7;
        public const int GL_MAX_RENDERBUFFER_SIZE = 0x84E8;
        public const int GL_INVALID_FRAMEBUFFER_OPERATION = 0x0506;

        public static readonly glActiveTexture glActiveTexture;
        public static readonly glAttachShader glAttachShader;
        public static readonly glBindAttribLocation glBindAttribLocation;
        public static readonly glBindBuffer glBindBuffer;
        public static readonly glBindFramebuffer glBindFramebuffer;
        public static readonly glBindRenderbuffer glBindRenderbuffer;
        public static readonly glBindTexture glBindTexture;
        public static readonly glBlendColor glBlendColor;
        public static readonly glBlendEquation glBlendEquation;
        public static readonly glBlendEquationSeparate glBlendEquationSeparate;
        public static readonly glBlendFunc glBlendFunc;
        public static readonly glBlendFuncSeparate glBlendFuncSeparate;
        public static readonly glBufferData glBufferData;
        public static readonly glBufferSubData glBufferSubData;
        public static readonly glCheckFramebufferStatus glCheckFramebufferStatus;
        public static readonly glClear glClear;
        public static readonly glClearColor glClearColor;
        public static readonly glClearDepthf glClearDepthf;
        public static readonly glClearStencil glClearStencil;
        public static readonly glColorMask glColorMask;
        public static readonly glCompileShader glCompileShader;
        public static readonly glCompressedTexImage2D glCompressedTexImage2D;
        public static readonly glCompressedTexSubImage2D glCompressedTexSubImage2D;
        public static readonly glCopyTexImage2D glCopyTexImage2D;
        public static readonly glCopyTexSubImage2D glCopyTexSubImage2D;
        public static readonly glCreateProgram glCreateProgram;
        public static readonly glCreateShader glCreateShader;
        public static readonly glCullFace glCullFace;
        public static readonly glDeleteBuffers glDeleteBuffers;
        public static readonly glDeleteFramebuffers glDeleteFramebuffers;
        public static readonly glDeleteProgram glDeleteProgram;
        public static readonly glDeleteRenderbuffers glDeleteRenderbuffers;
        public static readonly glDeleteShader glDeleteShader;
        public static readonly glDeleteTextures glDeleteTextures;
        public static readonly glDepthFunc glDepthFunc;
        public static readonly glDepthMask glDepthMask;
        public static readonly glDepthRangef glDepthRangef;

        // IF NOT FOUND glDepthRangef
        public static readonly glDepthRange glDepthRange;

        public static void DepthRange(float near, float far)
        {
            if (glDepthRangef != null) glDepthRangef(near, far);
            else glDepthRange(near, far);
        }

        public static readonly glDetachShader glDetachShader;
        public static readonly glDisable glDisable;
        public static readonly glDisableVertexAttribArray glDisableVertexAttribArray;
        public static readonly glDrawArrays glDrawArrays;
        public static readonly glDrawElements glDrawElements;
        public static readonly glEnable glEnable;
        public static readonly glEnableVertexAttribArray glEnableVertexAttribArray;
        public static readonly glFinish glFinish;
        public static readonly glFlush glFlush;
        public static readonly glFramebufferRenderbuffer glFramebufferRenderbuffer;
        public static readonly glFramebufferTexture2D glFramebufferTexture2D;
        public static readonly glFrontFace glFrontFace;
        public static readonly glGenBuffers glGenBuffers;
        public static readonly glGenerateMipmap glGenerateMipmap;
        public static readonly glGenFramebuffers glGenFramebuffers;
        public static readonly glGenRenderbuffers glGenRenderbuffers;

        //static public uint glGenTexture()
        //{
        //	uint Texture;
        //	glGenTextures(1, &Texture);
        //	return Texture;
        //}

        public static int glGetInteger(int Name)
        {
            int Out = 0;
            glGetIntegerv(Name, &Out);
            return Out;
        }

        public static string GetString(int Name)
        {
            return Marshal.PtrToStringAnsi(new GLintptr(glGetString(Name)));
        }

        public static readonly glGenTextures glGenTextures;
        public static readonly glGetActiveAttrib glGetActiveAttrib;
        public static readonly glGetActiveUniform glGetActiveUniform;
        public static readonly glGetAttachedShaders glGetAttachedShaders;
        public static readonly glGetAttribLocation glGetAttribLocation;
        public static readonly glGetBooleanv glGetBooleanv;
        public static readonly glGetBufferParameteriv glGetBufferParameteriv;
        public static readonly glGetError glGetError;
        public static readonly glGetFloatv glGetFloatv;
        public static readonly glGetFramebufferAttachmentParameteriv glGetFramebufferAttachmentParameteriv;
        public static readonly glGetIntegerv glGetIntegerv;
        public static readonly glGetProgramiv glGetProgramiv;
        public static readonly glGetProgramInfoLog glGetProgramInfoLog;
        public static readonly glGetRenderbufferParameteriv glGetRenderbufferParameteriv;
        public static readonly glGetShaderiv glGetShaderiv;
        public static readonly glGetShaderInfoLog glGetShaderInfoLog;
        public static readonly glGetShaderPrecisionFormat glGetShaderPrecisionFormat;
        public static readonly glGetShaderSource glGetShaderSource;
        public static readonly glGetString glGetString;
        public static readonly glGetTexParameterfv glGetTexParameterfv;
        public static readonly glGetTexParameteriv glGetTexParameteriv;
        public static readonly glGetUniformfv glGetUniformfv;
        public static readonly glGetUniformiv glGetUniformiv;
        public static readonly glGetUniformLocation glGetUniformLocation;
        public static readonly glGetVertexAttribfv glGetVertexAttribfv;
        public static readonly glGetVertexAttribiv glGetVertexAttribiv;
        public static readonly glGetVertexAttribPointerv glGetVertexAttribPointerv;
        public static readonly glHint glHint;
        public static readonly glIsBuffer glIsBuffer;
        public static readonly glIsEnabled glIsEnabled;
        public static readonly glIsFramebuffer glIsFramebuffer;
        public static readonly glIsProgram glIsProgram;
        public static readonly glIsRenderbuffer glIsRenderbuffer;
        public static readonly glIsShader glIsShader;
        public static readonly glIsTexture glIsTexture;
        public static readonly glLineWidth glLineWidth;
        public static readonly glLinkProgram glLinkProgram;
        public static readonly glPixelStorei glPixelStorei;
        public static readonly glPolygonOffset glPolygonOffset;
        public static readonly glReadPixels glReadPixels;
        public static readonly glReleaseShaderCompiler glReleaseShaderCompiler;
        public static readonly glRenderbufferStorage glRenderbufferStorage;
        public static readonly glSampleCoverage glSampleCoverage;
        public static readonly glScissor glScissor;
        public static readonly glShaderBinary glShaderBinary;
        public static readonly glShaderSource glShaderSource;
        public static readonly glStencilFunc glStencilFunc;
        public static readonly glStencilFuncSeparate glStencilFuncSeparate;
        public static readonly glStencilMask glStencilMask;
        public static readonly glStencilMaskSeparate glStencilMaskSeparate;
        public static readonly glStencilOp glStencilOp;
        public static readonly glStencilOpSeparate glStencilOpSeparate;
        public static readonly glTexImage2D glTexImage2D;
        public static readonly glTexParameterf glTexParameterf;
        public static readonly glTexParameterfv glTexParameterfv;
        public static readonly glTexParameteri glTexParameteri;
        public static readonly glTexParameteriv glTexParameteriv;
        public static readonly glTexSubImage2D glTexSubImage2D;
        public static readonly glUniform1f glUniform1f;
        public static readonly glUniform1fv glUniform1fv;
        public static readonly glUniform1i glUniform1i;
        public static readonly glUniform1iv glUniform1iv;
        public static readonly glUniform2f glUniform2f;
        public static readonly glUniform2fv glUniform2fv;
        public static readonly glUniform2i glUniform2i;
        public static readonly glUniform2iv glUniform2iv;
        public static readonly glUniform3f glUniform3f;
        public static readonly glUniform3fv glUniform3fv;
        public static readonly glUniform3i glUniform3i;
        public static readonly glUniform3iv glUniform3iv;
        public static readonly glUniform4f glUniform4f;
        public static readonly glUniform4fv glUniform4fv;
        public static readonly glUniform4i glUniform4i;
        public static readonly glUniform4iv glUniform4iv;
        public static readonly glUniformMatrix2fv glUniformMatrix2fv;
        public static readonly glUniformMatrix3fv glUniformMatrix3fv;
        public static readonly glUniformMatrix4fv glUniformMatrix4fv;
        public static readonly glUseProgram glUseProgram;
        public static readonly glValidateProgram glValidateProgram;
        public static readonly glVertexAttrib1f glVertexAttrib1f;
        public static readonly glVertexAttrib1fv glVertexAttrib1fv;
        public static readonly glVertexAttrib2f glVertexAttrib2f;
        public static readonly glVertexAttrib2fv glVertexAttrib2fv;
        public static readonly glVertexAttrib3f glVertexAttrib3f;
        public static readonly glVertexAttrib3fv glVertexAttrib3fv;
        public static readonly glVertexAttrib4f glVertexAttrib4f;
        public static readonly glVertexAttrib4fv glVertexAttrib4fv;
        public static readonly glVertexAttribPointer glVertexAttribPointer;
        public static readonly glViewport glViewport;

        public static void ClearError()
        {
            while (glGetError() != GL_NO_ERROR)
            {
            }
        }

        [DebuggerHidden]
        public static void CheckError()
        {
            try
            {
                var Error = glGetError();
                if (Error != GL_NO_ERROR) throw (new Exception(String.Format("glError: 0x{0:X4}", Error)));
            }
            finally
            {
                ClearError();
            }
        }

        public static bool EnableDisable(int EnableCap, bool EnableDisable)
        {
            if (EnableDisable)
            {
                glEnable(EnableCap);
            }
            else
            {
                glDisable(EnableCap);
            }
            return EnableDisable;
        }

        // REMOVE! Not available in OpenGL|ES
        [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public delegate void glGetTexImage_(GLenum texture, GLint level, GLenum format, GLenum type, void* img);

        public static readonly glGetTexImage_ glGetTexImage;

        public const int GL_MAJOR_VERSION = 0x821B;
        public const int GL_MINOR_VERSION = 0x821C;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glActiveTexture(GLenum texture);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glAttachShader(GLuint program, GLuint shader);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBindAttribLocation(GLuint program, GLuint index, string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBindBuffer(GLenum target, GLuint buffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBindFramebuffer(GLenum target, GLuint framebuffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBindRenderbuffer(GLenum target, GLuint renderbuffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBindTexture(GLenum target, GLuint texture);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBlendColor(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBlendEquation(GLenum mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBlendEquationSeparate(GLenum modeRGB, GLenum modeAlpha);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBlendFunc(GLenum sfactor, GLenum dfactor);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glBlendFuncSeparate(GLenum srcRGB, GLenum dstRGB, GLenum srcAlpha, GLenum dstAlpha);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glBufferData(GLenum target, GLsizeiptr size, void* data, GLenum usage);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glBufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, void* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLenum glCheckFramebufferStatus(GLenum target);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glClear(GLbitfield mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glClearColor(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glClearDepthf(GLclampf depth);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glClearStencil(GLint s);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glColorMask(GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glCompileShader(GLuint shader);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glCompressedTexImage2D(GLenum target, GLint level, GLenum internalformat, GLsizei width,
        GLsizei height, GLint border, GLsizei imageSize, void* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glCompressedTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset,
        GLsizei width, GLsizei height, GLenum format, GLsizei imageSize, void* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glCopyTexImage2D(GLenum target, GLint level, GLenum internalformat, GLint x, GLint y,
        GLsizei width, GLsizei height, GLint border);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glCopyTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x,
        GLint y, GLsizei width, GLsizei height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLuint glCreateProgram();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLuint glCreateShader(GLenum type);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glCullFace(GLenum mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glDeleteBuffers(GLsizei n, GLuint* buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glDeleteFramebuffers(GLsizei n, GLuint* framebuffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDeleteProgram(GLuint program);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glDeleteRenderbuffers(GLsizei n, GLuint* renderbuffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDeleteShader(GLuint shader);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glDeleteTextures(GLsizei n, GLuint* textures);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDepthFunc(GLenum func);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDepthMask(GLboolean flag);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDepthRangef(GLclampf zNear, GLclampf zFar);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDepthRange(double zNear, double zFar);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDetachShader(GLuint program, GLuint shader);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDisable(GLenum cap);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDisableVertexAttribArray(GLuint index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glDrawArrays(GLenum mode, GLint first, GLsizei count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glDrawElements(GLenum mode, GLsizei count, GLenum type, void* indices);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glEnable(GLenum cap);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glEnableVertexAttribArray(GLuint index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glFinish();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glFlush();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glFramebufferRenderbuffer(GLenum target, GLenum attachment, GLenum renderbuffertarget,
        GLuint renderbuffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glFramebufferTexture2D(GLenum target, GLenum attachment, GLenum textarget,
        GLuint texture, GLint level);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glFrontFace(GLenum mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGenBuffers(GLsizei n, GLuint* buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glGenerateMipmap(GLenum target);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGenFramebuffers(GLsizei n, GLuint* framebuffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGenRenderbuffers(GLsizei n, GLuint* renderbuffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGenTextures(GLsizei n, GLuint* textures);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetActiveAttrib(GLuint program, GLuint index, GLsizei bufsize, GLsizei* length,
        GLint* size, GLenum* type, GLchar* name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetActiveUniform(GLuint program, GLuint index, GLsizei bufsize, GLsizei* length,
        GLint* size, GLenum* type, GLchar* name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetAttachedShaders(GLuint program, GLsizei maxcount, GLsizei* count, GLuint* shaders);

    //[UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity] unsafe public delegate GLuint glGetAttribLocation(GLuint program, GLchar* name);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public delegate int glGetAttribLocation(GLuint program, string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetBooleanv(GLenum pname, GLboolean* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetBufferParameteriv(GLenum target, GLenum pname, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLenum glGetError();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetFloatv(GLenum pname, GLfloat* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetFramebufferAttachmentParameteriv(GLenum target, GLenum attachment, GLenum pname,
        GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetIntegerv(GLenum pname, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetProgramiv(GLuint program, GLenum pname, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetProgramInfoLog(GLuint program, GLsizei bufsize, GLsizei* length, GLchar* infolog);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetRenderbufferParameteriv(GLenum target, GLenum pname, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetShaderiv(GLuint shader, GLenum pname, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetShaderInfoLog(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* infolog);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetShaderPrecisionFormat(GLenum shadertype, GLenum precisiontype, GLint* range,
        GLint* precision);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetShaderSource(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* source);

    //[System.CLSCompliant(false)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate GLubyte* glGetString(GLenum name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetTexParameterfv(GLenum target, GLenum pname, GLfloat* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetTexParameteriv(GLenum target, GLenum pname, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetUniformfv(GLuint program, GLint location, GLfloat* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetUniformiv(GLuint program, GLint location, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate int glGetUniformLocation(GLuint program, string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetVertexAttribfv(GLuint index, GLenum pname, GLfloat* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetVertexAttribiv(GLuint index, GLenum pname, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glGetVertexAttribPointerv(GLuint index, GLenum pname, void** pointer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glHint(GLenum target, GLenum mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLboolean glIsBuffer(GLuint buffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLboolean glIsEnabled(GLenum cap);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLboolean glIsFramebuffer(GLuint framebuffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLboolean glIsProgram(GLuint program);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLboolean glIsRenderbuffer(GLuint renderbuffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLboolean glIsShader(GLuint shader);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate GLboolean glIsTexture(GLuint texture);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glLineWidth(GLfloat width);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glLinkProgram(GLuint program);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glPixelStorei(GLenum pname, GLint param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glPolygonOffset(GLfloat factor, GLfloat units);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glReadPixels(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format,
        GLenum type, void* pixels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glReleaseShaderCompiler();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glRenderbufferStorage(GLenum target, GLenum internalformat, GLsizei width,
        GLsizei height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glSampleCoverage(GLclampf value, GLboolean invert);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glScissor(GLint x, GLint y, GLsizei width, GLsizei height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glShaderBinary(GLsizei n, GLuint* shaders, GLenum binaryformat, void* binary,
        GLsizei length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glShaderSource(GLuint shader, GLsizei count, GLchar** @string, GLint* length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glStencilFunc(GLenum func, GLint @ref, GLuint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glStencilFuncSeparate(GLenum face, GLenum func, GLint @ref, GLuint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glStencilMask(GLuint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glStencilMaskSeparate(GLenum face, GLuint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glStencilOp(GLenum fail, GLenum zfail, GLenum zpass);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glStencilOpSeparate(GLenum face, GLenum fail, GLenum zfail, GLenum zpass);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glTexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width,
        GLsizei height, GLint border, GLenum format, GLenum type, void* pixels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glTexParameterf(GLenum target, GLenum pname, GLfloat param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glTexParameterfv(GLenum target, GLenum pname, GLfloat* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glTexParameteri(GLenum target, GLenum pname, GLint param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glTexParameteriv(GLenum target, GLenum pname, GLint* @params);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width,
        GLsizei height, GLenum format, GLenum type, void* pixels);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUniform1f(GLint location, GLfloat x);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniform1fv(GLint location, GLsizei count, GLfloat* v);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUniform1i(GLint location, GLint x);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniform1iv(GLint location, GLsizei count, GLint* v);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUniform2f(GLint location, GLfloat x, GLfloat y);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniform2fv(GLint location, GLsizei count, GLfloat* v);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUniform2i(GLint location, GLint x, GLint y);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniform2iv(GLint location, GLsizei count, GLint* v);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUniform3f(GLint location, GLfloat x, GLfloat y, GLfloat z);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniform3fv(GLint location, GLsizei count, GLfloat* v);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUniform3i(GLint location, GLint x, GLint y, GLint z);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniform3iv(GLint location, GLsizei count, GLint* v);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUniform4f(GLint location, GLfloat x, GLfloat y, GLfloat z, GLfloat w);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniform4fv(GLint location, GLsizei count, GLfloat* v);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUniform4i(GLint location, GLint x, GLint y, GLint z, GLint w);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniform4iv(GLint location, GLsizei count, GLint* v);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniformMatrix2fv(GLint location, GLsizei count, GLboolean transpose, GLfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniformMatrix3fv(GLint location, GLsizei count, GLboolean transpose, GLfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glUniformMatrix4fv(GLint location, GLsizei count, GLboolean transpose, GLfloat* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glUseProgram(GLuint program);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glValidateProgram(GLuint program);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glVertexAttrib1f(GLuint indx, GLfloat x);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glVertexAttrib1fv(GLuint indx, GLfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glVertexAttrib2f(GLuint indx, GLfloat x, GLfloat y);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glVertexAttrib2fv(GLuint indx, GLfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glVertexAttrib3f(GLuint indx, GLfloat x, GLfloat y, GLfloat z);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glVertexAttrib3fv(GLuint indx, GLfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glVertexAttrib4f(GLuint indx, GLfloat x, GLfloat y, GLfloat z, GLfloat w);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glVertexAttrib4fv(GLuint indx, GLfloat* values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public unsafe delegate void glVertexAttribPointer(GLuint indx, GLint size, GLenum type, GLboolean normalized,
        GLsizei stride, void* ptr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    public delegate void glViewport(GLint x, GLint y, GLsizei width, GLsizei height);
}