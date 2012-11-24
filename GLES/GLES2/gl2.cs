using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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

namespace GLES
{
	unsafe public partial class GL
	{
		/* $Revision: 10602 $ on $Date:: 2010-03-04 22:35:34 -0800 #$ */

		//#include <GLES2/gl2platform.h>

		/*
		 * This document is licensed under the SGI Free Software B License Version
		 * 2.0. For details, see http://oss.sgi.com/projects/FreeB/ .
		 */

		/*-------------------------------------------------------------------------
		 * Data type definitions
		 *-----------------------------------------------------------------------*/

		//typedef void             GLvoid;
		//typedef char             GLchar;
		//typedef unsigned int     GLenum;
		//typedef unsigned char    GLboolean;
		//typedef unsigned int     GLbitfield;
		//typedef khronos_int8_t   GLbyte;
		//typedef short            GLshort;
		//typedef int              GLint;
		//typedef int              GLsizei;
		//typedef khronos_uint8_t  GLubyte;
		//typedef unsigned short   GLushort;
		//typedef unsigned int     GLuint;
		//typedef khronos_float_t  GLfloat;
		//typedef khronos_float_t  GLclampf;
		//typedef khronos_int32_t  GLfixed;

		/* GL types for handling large vertex buffer objects */
		//typedef khronos_intptr_t GLintptr;
		//typedef khronos_ssize_t  GLsizeiptr;

		/* OpenGL ES core versions */
		public const int GL_ES_VERSION_2_0 = 1;

		/* ClearBufferMask */
		public const int GL_DEPTH_BUFFER_BIT = 0x00000100;
		public const int GL_STENCIL_BUFFER_BIT = 0x00000400;
		public const int GL_COLOR_BUFFER_BIT = 0x00004000;

		/* Boolean */
		public const bool GL_FALSE = false;
		public const bool GL_TRUE = true;

		/* BeginMode */
		public const int GL_POINTS = 0x0000;
		public const int GL_LINES = 0x0001;
		public const int GL_LINE_LOOP = 0x0002;
		public const int GL_LINE_STRIP = 0x0003;
		public const int GL_TRIANGLES = 0x0004;
		public const int GL_TRIANGLE_STRIP = 0x0005;
		public const int GL_TRIANGLE_FAN = 0x0006;

		/* AlphaFunction (not supported in ES20) */
		/*      GL_NEVER */
		/*      GL_LESS */
		/*      GL_EQUAL */
		/*      GL_LEQUAL */
		/*      GL_GREATER */
		/*      GL_NOTEQUAL */
		/*      GL_GEQUAL */
		/*      GL_ALWAYS */

		/* BlendingFactorDest */
		public const int GL_ZERO = 0;
		public const int GL_ONE = 1;
		public const int GL_SRC_COLOR = 0x0300;
		public const int GL_ONE_MINUS_SRC_COLOR = 0x0301;
		public const int GL_SRC_ALPHA = 0x0302;
		public const int GL_ONE_MINUS_SRC_ALPHA = 0x0303;
		public const int GL_DST_ALPHA = 0x0304;
		public const int GL_ONE_MINUS_DST_ALPHA = 0x0305;

		/* BlendingFactorSrc */
		/*      GL_ZERO */
		/*      GL_ONE */
		public const int GL_DST_COLOR = 0x0306;
		public const int GL_ONE_MINUS_DST_COLOR = 0x0307;
		public const int GL_SRC_ALPHA_SATURATE = 0x0308;
		/*      GL_SRC_ALPHA */
		/*      GL_ONE_MINUS_SRC_ALPHA */
		/*      GL_DST_ALPHA */
		/*      GL_ONE_MINUS_DST_ALPHA */

		/* BlendEquationSeparate */
		public const int GL_FUNC_ADD = 0x8006;
		public const int GL_BLEND_EQUATION = 0x8009;
		public const int GL_BLEND_EQUATION_RGB = 0x8009;    /* same as BLEND_EQUATION */
		public const int GL_BLEND_EQUATION_ALPHA = 0x883D;

		/* BlendSubtract */
		public const int GL_FUNC_SUBTRACT = 0x800A;
		public const int GL_FUNC_REVERSE_SUBTRACT = 0x800B;

		/* Separate Blend Functions */
		public const int GL_BLEND_DST_RGB = 0x80C8;
		public const int GL_BLEND_SRC_RGB = 0x80C9;
		public const int GL_BLEND_DST_ALPHA = 0x80CA;
		public const int GL_BLEND_SRC_ALPHA = 0x80CB;
		public const int GL_CONSTANT_COLOR = 0x8001;
		public const int GL_ONE_MINUS_CONSTANT_COLOR = 0x8002;
		public const int GL_CONSTANT_ALPHA = 0x8003;
		public const int GL_ONE_MINUS_CONSTANT_ALPHA = 0x8004;
		public const int GL_BLEND_COLOR = 0x8005;

		/* Buffer Objects */
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

		/* CullFaceMode */
		public const int GL_FRONT = 0x0404;
		public const int GL_BACK = 0x0405;
		public const int GL_FRONT_AND_BACK = 0x0408;

		/* DepthFunction */
		/*      GL_NEVER */
		/*      GL_LESS */
		/*      GL_EQUAL */
		/*      GL_LEQUAL */
		/*      GL_GREATER */
		/*      GL_NOTEQUAL */
		/*      GL_GEQUAL */
		/*      GL_ALWAYS */

		/* EnableCap */
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

		/* ErrorCode */
		public const int GL_NO_ERROR = 0;
		public const int GL_INVALID_ENUM = 0x0500;
		public const int GL_INVALID_VALUE = 0x0501;
		public const int GL_INVALID_OPERATION = 0x0502;
		public const int GL_OUT_OF_MEMORY = 0x0505;

		/* FrontFaceDirection */
		public const int GL_CW = 0x0900;
		public const int GL_CCW = 0x0901;

		/* GetPName */
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
		/*      GL_SCISSOR_TEST */
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
		/*      GL_POLYGON_OFFSET_FILL */
		public const int GL_POLYGON_OFFSET_FACTOR = 0x8038;
		public const int GL_TEXTURE_BINDING_2D = 0x8069;
		public const int GL_SAMPLE_BUFFERS = 0x80A8;
		public const int GL_SAMPLES = 0x80A9;
		public const int GL_SAMPLE_COVERAGE_VALUE = 0x80AA;
		public const int GL_SAMPLE_COVERAGE_INVERT = 0x80AB;

		/* GetTextureParameter */
		/*      GL_TEXTURE_MAG_FILTER */
		/*      GL_TEXTURE_MIN_FILTER */
		/*      GL_TEXTURE_WRAP_S */
		/*      GL_TEXTURE_WRAP_T */

		public const int GL_NUM_COMPRESSED_TEXTURE_FORMATS = 0x86A2;
		public const int GL_COMPRESSED_TEXTURE_FORMATS = 0x86A3;

		/* HintMode */
		public const int GL_DONT_CARE = 0x1100;
		public const int GL_FASTEST = 0x1101;
		public const int GL_NICEST = 0x1102;

		/* HintTarget */
		public const int GL_GENERATE_MIPMAP_HINT = 0x8192;

		/* DataType */
		public const int GL_BYTE = 0x1400;
		public const int GL_UNSIGNED_BYTE = 0x1401;
		public const int GL_SHORT = 0x1402;
		public const int GL_UNSIGNED_SHORT = 0x1403;
		public const int GL_INT = 0x1404;
		public const int GL_UNSIGNED_INT = 0x1405;
		public const int GL_FLOAT = 0x1406;
		public const int GL_FIXED = 0x140C;

		/* PixelFormat */
		public const int GL_DEPTH_COMPONENT = 0x1902;
		public const int GL_ALPHA = 0x1906;
		public const int GL_RGB = 0x1907;
		public const int GL_RGBA = 0x1908;
		public const int GL_LUMINANCE = 0x1909;
		public const int GL_LUMINANCE_ALPHA = 0x190A;

		/* PixelType */
		/*      GL_UNSIGNED_BYTE */
		public const int GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033;
		public const int GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034;
		public const int GL_UNSIGNED_SHORT_5_6_5 = 0x8363;

		/* Shaders */
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

		/* StencilFunction */
		public const int GL_NEVER = 0x0200;
		public const int GL_LESS = 0x0201;
		public const int GL_EQUAL = 0x0202;
		public const int GL_LEQUAL = 0x0203;
		public const int GL_GREATER = 0x0204;
		public const int GL_NOTEQUAL = 0x0205;
		public const int GL_GEQUAL = 0x0206;
		public const int GL_ALWAYS = 0x0207;

		/* StencilOp */
		/*      GL_ZERO */
		public const int GL_KEEP = 0x1E00;
		public const int GL_REPLACE = 0x1E01;
		public const int GL_INCR = 0x1E02;
		public const int GL_DECR = 0x1E03;
		public const int GL_INVERT = 0x150A;
		public const int GL_INCR_WRAP = 0x8507;
		public const int GL_DECR_WRAP = 0x8508;

		/* StringName */
		public const int GL_VENDOR = 0x1F00;
		public const int GL_RENDERER = 0x1F01;
		public const int GL_VERSION = 0x1F02;
		public const int GL_EXTENSIONS = 0x1F03;

		/* TextureMagFilter */
		public const int GL_NEAREST = 0x2600;
		public const int GL_LINEAR = 0x2601;

		/* TextureMinFilter */
		/*      GL_NEAREST */
		/*      GL_LINEAR */
		public const int GL_NEAREST_MIPMAP_NEAREST = 0x2700;
		public const int GL_LINEAR_MIPMAP_NEAREST = 0x2701;
		public const int GL_NEAREST_MIPMAP_LINEAR = 0x2702;
		public const int GL_LINEAR_MIPMAP_LINEAR = 0x2703;

		/* TextureParameterName */
		public const int GL_TEXTURE_MAG_FILTER = 0x2800;
		public const int GL_TEXTURE_MIN_FILTER = 0x2801;
		public const int GL_TEXTURE_WRAP_S = 0x2802;
		public const int GL_TEXTURE_WRAP_T = 0x2803;

		/* TextureTarget */
		/*      GL_TEXTURE_2D */
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

		/* TextureUnit */
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

		/* TextureWrapMode */
		public const int GL_REPEAT = 0x2901;
		public const int GL_CLAMP_TO_EDGE = 0x812F;
		public const int GL_MIRRORED_REPEAT = 0x8370;

		/* Uniform Types */
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

		/* Vertex Arrays */
		public const int GL_VERTEX_ATTRIB_ARRAY_ENABLED = 0x8622;
		public const int GL_VERTEX_ATTRIB_ARRAY_SIZE = 0x8623;
		public const int GL_VERTEX_ATTRIB_ARRAY_STRIDE = 0x8624;
		public const int GL_VERTEX_ATTRIB_ARRAY_TYPE = 0x8625;
		public const int GL_VERTEX_ATTRIB_ARRAY_NORMALIZED = 0x886A;
		public const int GL_VERTEX_ATTRIB_ARRAY_POINTER = 0x8645;
		public const int GL_VERTEX_ATTRIB_ARRAY_BUFFER_BINDING = 0x889F;

		/* Read Format */
		public const int GL_IMPLEMENTATION_COLOR_READ_TYPE = 0x8B9A;
		public const int GL_IMPLEMENTATION_COLOR_READ_FORMAT = 0x8B9B;

		/* Shader Source */
		public const int GL_COMPILE_STATUS = 0x8B81;
		public const int GL_INFO_LOG_LENGTH = 0x8B84;
		public const int GL_SHADER_SOURCE_LENGTH = 0x8B88;
		public const int GL_SHADER_COMPILER = 0x8DFA;

		/* Shader Binary */
		public const int GL_SHADER_BINARY_FORMATS = 0x8DF8;
		public const int GL_NUM_SHADER_BINARY_FORMATS = 0x8DF9;

		/* Shader Precision-Specified Types */
		public const int GL_LOW_FLOAT = 0x8DF0;
		public const int GL_MEDIUM_FLOAT = 0x8DF1;
		public const int GL_HIGH_FLOAT = 0x8DF2;
		public const int GL_LOW_INT = 0x8DF3;
		public const int GL_MEDIUM_INT = 0x8DF4;
		public const int GL_HIGH_INT = 0x8DF5;

		/* Framebuffer Object. */
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

		/*-------------------------------------------------------------------------
		 * GL core functions.
		 *-----------------------------------------------------------------------*/

		[DllImport("libGLESv2")] static public extern void glActiveTexture(GLenum texture);
		[DllImport("libGLESv2")] static public extern void glAttachShader(GLuint program, GLuint shader);
		[DllImport("libGLESv2")] static public extern void glBindAttribLocation(GLuint program, GLuint index, string name);
		[DllImport("libGLESv2")] static public extern void glBindBuffer(GLenum target, GLuint buffer);
		[DllImport("libGLESv2")] static public extern void glBindFramebuffer(GLenum target, GLuint framebuffer);
		[DllImport("libGLESv2")] static public extern void glBindRenderbuffer(GLenum target, GLuint renderbuffer);
		[DllImport("libGLESv2")] static public extern void glBindTexture(GLenum target, GLuint texture);
		[DllImport("libGLESv2")] static public extern void glBlendColor(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);
		[DllImport("libGLESv2")] static public extern void glBlendEquation(GLenum mode);
		[DllImport("libGLESv2")] static public extern void glBlendEquationSeparate(GLenum modeRGB, GLenum modeAlpha);
		[DllImport("libGLESv2")] static public extern void glBlendFunc(GLenum sfactor, GLenum dfactor);
		[DllImport("libGLESv2")] static public extern void glBlendFuncSeparate(GLenum srcRGB, GLenum dstRGB, GLenum srcAlpha, GLenum dstAlpha);
		[DllImport("libGLESv2")] static public extern void glBufferData(GLenum target, GLsizeiptr size, void* data, GLenum usage);
		[DllImport("libGLESv2")] static public extern void glBufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, void* data);
		[DllImport("libGLESv2")] static public extern GLenum glCheckFramebufferStatus(GLenum target);
		[DllImport("libGLESv2")] static public extern void glClear(GLbitfield mask);
		[DllImport("libGLESv2")] static public extern void glClearColor(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);
		[DllImport("libGLESv2")] static public extern void glClearDepthf(GLclampf depth);
		[DllImport("libGLESv2")] static public extern void glClearStencil(GLint s);
		[DllImport("libGLESv2")] static public extern void glColorMask(GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha);
		[DllImport("libGLESv2")] static public extern void glCompileShader(GLuint shader);
		[DllImport("libGLESv2")] static public extern void glCompressedTexImage2D(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, GLsizei imageSize, void* data);
		[DllImport("libGLESv2")] static public extern void glCompressedTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLsizei imageSize, void* data);
		[DllImport("libGLESv2")] static public extern void glCopyTexImage2D(GLenum target, GLint level, GLenum internalformat, GLint x, GLint y, GLsizei width, GLsizei height, GLint border);
		[DllImport("libGLESv2")] static public extern void glCopyTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width, GLsizei height);
		[DllImport("libGLESv2")] static public extern GLuint glCreateProgram();
		[DllImport("libGLESv2")] static public extern GLuint glCreateShader(GLenum type);
		[DllImport("libGLESv2")] static public extern void glCullFace(GLenum mode);
		[DllImport("libGLESv2")] static public extern void glDeleteBuffers(GLsizei n, GLuint* buffers);
		[DllImport("libGLESv2")] static public extern void glDeleteFramebuffers(GLsizei n, GLuint* framebuffers);
		[DllImport("libGLESv2")] static public extern void glDeleteProgram(GLuint program);
		[DllImport("libGLESv2")] static public extern void glDeleteRenderbuffers(GLsizei n, GLuint* renderbuffers);
		[DllImport("libGLESv2")] static public extern void glDeleteShader(GLuint shader);
		[DllImport("libGLESv2")] static public extern void glDeleteTextures(GLsizei n, GLuint* textures);
		[DllImport("libGLESv2")] static public extern void glDepthFunc(GLenum func);
		[DllImport("libGLESv2")] static public extern void glDepthMask(GLboolean flag);
		[DllImport("libGLESv2")] static public extern void glDepthRangef(GLclampf zNear, GLclampf zFar);
		[DllImport("libGLESv2")] static public extern void glDetachShader(GLuint program, GLuint shader);
		[DllImport("libGLESv2")] static public extern void glDisable(GLenum cap);
		[DllImport("libGLESv2")] static public extern void glDisableVertexAttribArray(GLuint index);
		[DllImport("libGLESv2")] static public extern void glDrawArrays(GLenum mode, GLint first, GLsizei count);
		[DllImport("libGLESv2")] static public extern void glDrawElements(GLenum mode, GLsizei count, GLenum type, void* indices);
		[DllImport("libGLESv2")] static public extern void glEnable(GLenum cap);
		[DllImport("libGLESv2")] static public extern void glEnableVertexAttribArray(GLuint index);
		[DllImport("libGLESv2")] static public extern void glFinish();
		[DllImport("libGLESv2")] static public extern void glFlush();
		[DllImport("libGLESv2")] static public extern void glFramebufferRenderbuffer(GLenum target, GLenum attachment, GLenum renderbuffertarget, GLuint renderbuffer);
		[DllImport("libGLESv2")] static public extern void glFramebufferTexture2D(GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level);
		[DllImport("libGLESv2")] static public extern void glFrontFace(GLenum mode);
		[DllImport("libGLESv2")] static public extern void glGenBuffers(GLsizei n, GLuint* buffers);
		[DllImport("libGLESv2")] static public extern void glGenerateMipmap(GLenum target);
		[DllImport("libGLESv2")] static public extern void glGenFramebuffers(GLsizei n, GLuint* framebuffers);
		[DllImport("libGLESv2")] static public extern void glGenRenderbuffers(GLsizei n, GLuint* renderbuffers);
		[DllImport("libGLESv2")] static public extern void glGenTextures(GLsizei n, GLuint* textures);
		[DllImport("libGLESv2")] static public extern void glGetActiveAttrib(GLuint program, GLuint index, GLsizei bufsize, GLsizei* length, GLint* size, GLenum* type, GLchar* name);
		[DllImport("libGLESv2")] static public extern void glGetActiveUniform(GLuint program, GLuint index, GLsizei bufsize, GLsizei* length, GLint* size, GLenum* type, GLchar* name);
		[DllImport("libGLESv2")] static public extern void glGetAttachedShaders(GLuint program, GLsizei maxcount, GLsizei* count, GLuint* shaders);
		[DllImport("libGLESv2")] static public extern int glGetAttribLocation(GLuint program, GLchar* name);
		[DllImport("libGLESv2")] static public extern void glGetBooleanv(GLenum pname, GLboolean* @params);
		[DllImport("libGLESv2")] static public extern void glGetBufferParameteriv(GLenum target, GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern GLenum glGetError();
		[DllImport("libGLESv2")] static public extern void glGetFloatv(GLenum pname, GLfloat* @params);
		[DllImport("libGLESv2")] static public extern void glGetFramebufferAttachmentParameteriv(GLenum target, GLenum attachment, GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern void glGetIntegerv(GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern void glGetProgramiv(GLuint program, GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern void glGetProgramInfoLog(GLuint program, GLsizei bufsize, GLsizei* length, GLchar* infolog);
		[DllImport("libGLESv2")] static public extern void glGetRenderbufferParameteriv(GLenum target, GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern void glGetShaderiv(GLuint shader, GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern void glGetShaderInfoLog(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* infolog);
		[DllImport("libGLESv2")] static public extern void glGetShaderPrecisionFormat(GLenum shadertype, GLenum precisiontype, GLint* range, GLint* precision);
		[DllImport("libGLESv2")] static public extern void glGetShaderSource(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* source);
		[DllImport("libGLESv2")] static public extern GLubyte* glGetString(GLenum name);
		[DllImport("libGLESv2")] static public extern void glGetTexParameterfv(GLenum target, GLenum pname, GLfloat* @params);
		[DllImport("libGLESv2")] static public extern void glGetTexParameteriv(GLenum target, GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern void glGetUniformfv(GLuint program, GLint location, GLfloat* @params);
		[DllImport("libGLESv2")] static public extern void glGetUniformiv(GLuint program, GLint location, GLint* @params);
		[DllImport("libGLESv2")] static public extern int glGetUniformLocation(GLuint program, string name);
		[DllImport("libGLESv2")] static public extern void glGetVertexAttribfv(GLuint index, GLenum pname, GLfloat* @params);
		[DllImport("libGLESv2")] static public extern void glGetVertexAttribiv(GLuint index, GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern void glGetVertexAttribPointerv(GLuint index, GLenum pname, void** pointer);
		[DllImport("libGLESv2")] static public extern void glHint(GLenum target, GLenum mode);
		[DllImport("libGLESv2")] static public extern GLboolean glIsBuffer(GLuint buffer);
		[DllImport("libGLESv2")] static public extern GLboolean glIsEnabled(GLenum cap);
		[DllImport("libGLESv2")] static public extern GLboolean glIsFramebuffer(GLuint framebuffer);
		[DllImport("libGLESv2")] static public extern GLboolean glIsProgram(GLuint program);
		[DllImport("libGLESv2")] static public extern GLboolean glIsRenderbuffer(GLuint renderbuffer);
		[DllImport("libGLESv2")] static public extern GLboolean glIsShader(GLuint shader);
		[DllImport("libGLESv2")] static public extern GLboolean glIsTexture(GLuint texture);
		[DllImport("libGLESv2")] static public extern void glLineWidth(GLfloat width);
		[DllImport("libGLESv2")] static public extern void glLinkProgram(GLuint program);
		[DllImport("libGLESv2")] static public extern void glPixelStorei(GLenum pname, GLint param);
		[DllImport("libGLESv2")] static public extern void glPolygonOffset(GLfloat factor, GLfloat units);
		[DllImport("libGLESv2")] static public extern void glReadPixels(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, void* pixels);
		[DllImport("libGLESv2")] static public extern void glReleaseShaderCompiler();
		[DllImport("libGLESv2")] static public extern void glRenderbufferStorage(GLenum target, GLenum internalformat, GLsizei width, GLsizei height);
		[DllImport("libGLESv2")] static public extern void glSampleCoverage(GLclampf value, GLboolean invert);
		[DllImport("libGLESv2")] static public extern void glScissor(GLint x, GLint y, GLsizei width, GLsizei height);
		[DllImport("libGLESv2")] static public extern void glShaderBinary(GLsizei n, GLuint* shaders, GLenum binaryformat, void* binary, GLsizei length);
		[DllImport("libGLESv2")] static public extern void glShaderSource(GLuint shader, GLsizei count, GLchar** @string, GLint* length);
		[DllImport("libGLESv2")] static public extern void glStencilFunc(GLenum func, GLint @ref, GLuint mask);
		[DllImport("libGLESv2")] static public extern void glStencilFuncSeparate(GLenum face, GLenum func, GLint @ref, GLuint mask);
		[DllImport("libGLESv2")] static public extern void glStencilMask(GLuint mask);
		[DllImport("libGLESv2")] static public extern void glStencilMaskSeparate(GLenum face, GLuint mask);
		[DllImport("libGLESv2")] static public extern void glStencilOp(GLenum fail, GLenum zfail, GLenum zpass);
		[DllImport("libGLESv2")] static public extern void glStencilOpSeparate(GLenum face, GLenum fail, GLenum zfail, GLenum zpass);
		[DllImport("libGLESv2")] static public extern void glTexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, void* pixels);
		[DllImport("libGLESv2")] static public extern void glTexParameterf(GLenum target, GLenum pname, GLfloat param);
		[DllImport("libGLESv2")] static public extern void glTexParameterfv(GLenum target, GLenum pname, GLfloat* @params);
		[DllImport("libGLESv2")] static public extern void glTexParameteri(GLenum target, GLenum pname, GLint param);
		[DllImport("libGLESv2")] static public extern void glTexParameteriv(GLenum target, GLenum pname, GLint* @params);
		[DllImport("libGLESv2")] static public extern void glTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, void* pixels);
		[DllImport("libGLESv2")] static public extern void glUniform1f(GLint location, GLfloat x);
		[DllImport("libGLESv2")] static public extern void glUniform1fv(GLint location, GLsizei count, GLfloat* v);
		[DllImport("libGLESv2")] static public extern void glUniform1i(GLint location, GLint x);
		[DllImport("libGLESv2")] static public extern void glUniform1iv(GLint location, GLsizei count, GLint* v);
		[DllImport("libGLESv2")] static public extern void glUniform2f(GLint location, GLfloat x, GLfloat y);
		[DllImport("libGLESv2")] static public extern void glUniform2fv(GLint location, GLsizei count, GLfloat* v);
		[DllImport("libGLESv2")] static public extern void glUniform2i(GLint location, GLint x, GLint y);
		[DllImport("libGLESv2")] static public extern void glUniform2iv(GLint location, GLsizei count, GLint* v);
		[DllImport("libGLESv2")] static public extern void glUniform3f(GLint location, GLfloat x, GLfloat y, GLfloat z);
		[DllImport("libGLESv2")] static public extern void glUniform3fv(GLint location, GLsizei count, GLfloat* v);
		[DllImport("libGLESv2")] static public extern void glUniform3i(GLint location, GLint x, GLint y, GLint z);
		[DllImport("libGLESv2")] static public extern void glUniform3iv(GLint location, GLsizei count, GLint* v);
		[DllImport("libGLESv2")] static public extern void glUniform4f(GLint location, GLfloat x, GLfloat y, GLfloat z, GLfloat w);
		[DllImport("libGLESv2")] static public extern void glUniform4fv(GLint location, GLsizei count, GLfloat* v);
		[DllImport("libGLESv2")] static public extern void glUniform4i(GLint location, GLint x, GLint y, GLint z, GLint w);
		[DllImport("libGLESv2")] static public extern void glUniform4iv(GLint location, GLsizei count, GLint* v);
		[DllImport("libGLESv2")] static public extern void glUniformMatrix2fv(GLint location, GLsizei count, GLboolean transpose, GLfloat* value);
		[DllImport("libGLESv2")] static public extern void glUniformMatrix3fv(GLint location, GLsizei count, GLboolean transpose, GLfloat* value);
		[DllImport("libGLESv2")] static public extern void glUniformMatrix4fv(GLint location, GLsizei count, GLboolean transpose, GLfloat* value);
		[DllImport("libGLESv2")] static public extern void glUseProgram(GLuint program);
		[DllImport("libGLESv2")] static public extern void glValidateProgram(GLuint program);
		[DllImport("libGLESv2")] static public extern void glVertexAttrib1f(GLuint indx, GLfloat x);
		[DllImport("libGLESv2")] static public extern void glVertexAttrib1fv(GLuint indx, GLfloat* values);
		[DllImport("libGLESv2")] static public extern void glVertexAttrib2f(GLuint indx, GLfloat x, GLfloat y);
		[DllImport("libGLESv2")] static public extern void glVertexAttrib2fv(GLuint indx, GLfloat* values);
		[DllImport("libGLESv2")] static public extern void glVertexAttrib3f(GLuint indx, GLfloat x, GLfloat y, GLfloat z);
		[DllImport("libGLESv2")] static public extern void glVertexAttrib3fv(GLuint indx, GLfloat* values);
		[DllImport("libGLESv2")] static public extern void glVertexAttrib4f(GLuint indx, GLfloat x, GLfloat y, GLfloat z, GLfloat w);
		[DllImport("libGLESv2")] static public extern void glVertexAttrib4fv(GLuint indx, GLfloat* values);
		[DllImport("libGLESv2")] static public extern void glVertexAttribPointer(GLuint indx, GLint size, GLenum type, GLboolean normalized, GLsizei stride, void* ptr);
		[DllImport("libGLESv2")] static public extern void glViewport(GLint x, GLint y, GLsizei width, GLsizei height);

		// UTILS
		static public string glGetShaderInfoLog(GLuint shader)
		{
			int infoLen = 0;

			GL.glGetShaderiv(shader, GL.GL_INFO_LOG_LENGTH, &infoLen);

			if (infoLen > 1)
			{
				var infoLog = new byte[infoLen];
				//char* infoLog = malloc (sizeof(char) * infoLen );

				fixed (byte* infoLogPtr = infoLog)
				{
					GL.glGetShaderInfoLog(shader, infoLen, null, infoLogPtr);
				}
				return Encoding.ASCII.GetString(infoLog);
			}

			return null;
		}

		static public string glGetProgramInfoLog(GLuint programObject)
		{
			GLint infoLen = 0;

			GL.glGetProgramiv(programObject, GL_INFO_LOG_LENGTH, &infoLen);

			if (infoLen > 1)
			{
				var infoLog = new byte[infoLen];

				fixed (byte* infoLogPtr = infoLog)
				{
					GL.glGetProgramInfoLog(programObject, infoLen, null, infoLogPtr);
				}
				return Encoding.ASCII.GetString(infoLog);
			}

			return null;
		}

		static public bool glEnableDisable(GLenum state, bool enable)
		{
			if (enable)
			{
				GL.glEnable(state);
			}
			else
			{
				GL.glDisable(state);
			}

			return enable;
		}

		static public void glDeleteTexture(GLuint texture)
		{
			glDeleteTextures(1, &texture);
		}

		static public GLuint glGenTexture()
		{
			GLuint texture = 0;
			glGenTextures(1, &texture);
			return texture;
		}
	}
}
