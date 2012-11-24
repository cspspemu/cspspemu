using GLES;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
	public sealed unsafe partial class GpuImplOpenglEs
	{
		static readonly int[] StencilOperationTranslate = new[]
		{
			GL.GL_KEEP,
			GL.GL_ZERO,
			GL.GL_REPLACE,
			GL.GL_INVERT,
			GL.GL_INCR,
			GL.GL_DECR,
		};

		static readonly int[] StencilFunctionTranslate = new[]
		{
			GL.GL_NEVER,
			GL.GL_ALWAYS,
			GL.GL_EQUAL,
			GL.GL_NOTEQUAL,
			GL.GL_LESS,
			GL.GL_LEQUAL,
			GL.GL_GREATER,
			GL.GL_GEQUAL,
		};

		static readonly int[] DepthFunctionTranslate = new[]
		{
			GL.GL_NEVER,
			GL.GL_ALWAYS,
			GL.GL_EQUAL,
			GL.GL_NOTEQUAL,
			GL.GL_LESS,
			GL.GL_LEQUAL,
			GL.GL_GREATER,
			GL.GL_GEQUAL,
		};

		static readonly int[] BlendEquationTranslate = new[]
		{
			GL.GL_FUNC_ADD,
			GL.GL_FUNC_SUBTRACT,
			GL.GL_FUNC_REVERSE_SUBTRACT,
			GL.GL_FUNC_ADD, // MIN
			GL.GL_FUNC_ADD, // MAX
			GL.GL_FUNC_ADD, // ABS
		};

		static readonly int[] BlendFuncSrcTranslate = new[]
		{
			/// 0 GU_SRC_COLOR,
			GL.GL_SRC_COLOR,
			/// 1 GU_ONE_MINUS_SRC_COLOR,
			GL.GL_ONE_MINUS_SRC_COLOR,
			/// 2 GU_SRC_ALPHA,
			GL.GL_SRC_ALPHA,
			/// 3 GU_ONE_MINUS_SRC_ALPHA,
			GL.GL_ONE_MINUS_SRC_ALPHA,
			/// 4 -,
			GL.GL_DST_ALPHA,
			/// 5 -,
			GL.GL_ONE_MINUS_DST_ALPHA,
			/// 6 -,
			GL.GL_SRC_ALPHA,
			/// 7 -,
			GL.GL_ONE_MINUS_SRC_ALPHA,
			/// 8 -,
			GL.GL_DST_ALPHA,
			/// 9 -,
			GL.GL_ONE_MINUS_DST_ALPHA,
			/// 10 GU_FIX
			GL.GL_SRC_ALPHA,
		};

		static readonly int[] BlendFuncDstTranslate = new[]
		{
			/// 0 GU_DST_COLOR,
			GL.GL_DST_COLOR,
			/// 1 GU_ONE_MINUS_DST_COLOR,
			GL.GL_ONE_MINUS_DST_COLOR,
			/// 2 -,
			GL.GL_DST_ALPHA,
			/// 3 -,
			GL.GL_ONE_MINUS_DST_ALPHA,
			/// 4 GU_DST_ALPHA,
			GL.GL_DST_ALPHA,
			/// 5 GU_ONE_MINUS_DST_ALPHA,
			GL.GL_ONE_MINUS_DST_ALPHA,
			/// 6 -,
			GL.GL_SRC_ALPHA,
			/// 7 -,
			GL.GL_ONE_MINUS_SRC_ALPHA,
			/// 8 -,
			GL.GL_DST_ALPHA,
			/// 9 -,
			GL.GL_ONE_MINUS_DST_ALPHA,
			/// 10 GU_FIX
			GL.GL_ONE_MINUS_SRC_ALPHA,
		};

		/*
		static readonly TextureEnvMode[] TextureEnvModeTranslate = new[]
		{
			TextureEnvMode.Modulate,
			TextureEnvMode.Decal,
			TextureEnvMode.Blend,
			TextureEnvMode.Replace,
			TextureEnvMode.Add,
		};
		*/

		static readonly int[] PrimitiveTypeTranslate = new[]
		{
			GL.GL_POINTS,//Points = 0,
			GL.GL_LINES,//Lines = 1,
			GL.GL_LINE_STRIP,//LineStrip = 2,
			GL.GL_TRIANGLES,//Triangles = 3,
			GL.GL_TRIANGLE_STRIP,//TriangleStrip = 4,
			GL.GL_TRIANGLE_FAN,//TriangleFan = 5,
			GL.GL_TRIANGLE_STRIP,//Sprites = 6,
		};

		public struct GlPixelFormat
		{
			public GuPixelFormats GuPixelFormat;
			public int OpenglPixelType;
		};

		static public readonly GlPixelFormat[] GlPixelFormatList = new GlPixelFormat[]
		{
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_5650, OpenglPixelType = GL.GL_UNSIGNED_SHORT_5_6_5 },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_5551, OpenglPixelType = GL.GL_UNSIGNED_SHORT_5_5_5_1 },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_4444, OpenglPixelType = GL.GL_UNSIGNED_SHORT_4_4_4_4 },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_8888, OpenglPixelType = GL.GL_UNSIGNED_BYTE },
		};
	}
}
