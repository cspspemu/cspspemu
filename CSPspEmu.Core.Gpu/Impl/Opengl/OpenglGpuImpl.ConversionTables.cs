#if OPENTK
using OpenTK.Graphics.OpenGL;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public sealed partial class OpenglGpuImpl
	{
		static readonly StencilOp[] StencilOperationTranslate = new StencilOp[]
		{
			StencilOp.Keep,
			StencilOp.Zero,
			StencilOp.Replace,
			StencilOp.Invert,
			StencilOp.Incr,
			StencilOp.Decr,
		};

		static readonly StencilFunction[] StencilFunctionTranslate = new StencilFunction[]
		{
			StencilFunction.Never,
			StencilFunction.Always,
			StencilFunction.Equal,
			StencilFunction.Notequal,
			StencilFunction.Less,
			StencilFunction.Lequal,
			StencilFunction.Greater, 
			StencilFunction.Gequal
		};

		static readonly DepthFunction[] DepthFunctionTranslate = new DepthFunction[]
		{
			DepthFunction.Never,
			DepthFunction.Always,
			DepthFunction.Equal,
			DepthFunction.Notequal,
			DepthFunction.Less,
			DepthFunction.Lequal,
			DepthFunction.Greater, 
			DepthFunction.Gequal
		};

		static readonly BlendEquationMode[] BlendEquationTranslate = new BlendEquationMode[]
		{
			BlendEquationMode.FuncAdd,
			BlendEquationMode.FuncSubtract,
			BlendEquationMode.FuncReverseSubtract,
			BlendEquationMode.Min,
			BlendEquationMode.Max,
			BlendEquationMode.FuncAdd, /* ABS */
		};

		const int GL_ZERO = 0x0;
		const int GL_ONE = 0x1;
		const int GL_SRC_COLOR = 0x0300;
		const int GL_ONE_MINUS_SRC_COLOR = 0x0301;
		const int GL_SRC_ALPHA = 0x0302;
		const int GL_ONE_MINUS_SRC_ALPHA = 0x0303;
		const int GL_DST_ALPHA = 0x0304;
		const int GL_ONE_MINUS_DST_ALPHA = 0x0305;
		const int GL_DST_COLOR = 0x0306;
		const int GL_ONE_MINUS_DST_COLOR = 0x0307;
		const int GL_SRC_ALPHA_SATURATE = 0x0308;
		const int GL_CONSTANT_COLOR = 0x8001;
		const int GL_ONE_MINUS_CONSTANT_COLOR = 0x8002;


		/*
		// Source
		GU_SRC_COLOR = 0, GU_ONE_MINUS_SRC_COLOR = 1, GU_SRC_ALPHA = 2, GU_ONE_MINUS_SRC_ALPHA = 3,
		// Both?
		GU_FIX = 10
		}

		*/

		static readonly BlendingFactorSrc[] BlendFuncSrcTranslate = new BlendingFactorSrc[]
		{
			// 0 GU_SRC_COLOR,
			(BlendingFactorSrc)GL_SRC_COLOR,
			// 1 GU_ONE_MINUS_SRC_COLOR,
			(BlendingFactorSrc)GL_ONE_MINUS_SRC_COLOR,
			// 2 GU_SRC_ALPHA,
			(BlendingFactorSrc)GL_SRC_ALPHA,
			// 3 GU_ONE_MINUS_SRC_ALPHA,
			(BlendingFactorSrc)GL_ONE_MINUS_SRC_ALPHA,
			// 4 -,
			(BlendingFactorSrc)GL_DST_ALPHA,
			// 5 -,
			(BlendingFactorSrc)GL_ONE_MINUS_DST_ALPHA,
			// 6 -,
			(BlendingFactorSrc)GL_SRC_ALPHA,
			// 7 -,
			(BlendingFactorSrc)GL_ONE_MINUS_SRC_ALPHA,
			// 8 -,
			(BlendingFactorSrc)GL_DST_ALPHA,
			// 9 -,
			(BlendingFactorSrc)GL_ONE_MINUS_DST_ALPHA,
			// 10 GU_FIX
			(BlendingFactorSrc)GL_SRC_ALPHA,
		};

		static readonly BlendingFactorDest[] BlendFuncDstTranslate = new BlendingFactorDest[]
		{
			// 0 GU_DST_COLOR,
			(BlendingFactorDest)GL_DST_COLOR,
			// 1 GU_ONE_MINUS_DST_COLOR,
			(BlendingFactorDest)GL_ONE_MINUS_DST_COLOR,
			// 2 -,
			(BlendingFactorDest)GL_DST_ALPHA,
			// 3 -,
			(BlendingFactorDest)GL_ONE_MINUS_DST_ALPHA,
			// 4 GU_DST_ALPHA,
			(BlendingFactorDest)GL_DST_ALPHA,
			// 5 GU_ONE_MINUS_DST_ALPHA,
			(BlendingFactorDest)GL_ONE_MINUS_DST_ALPHA,
			// 6 -,
			(BlendingFactorDest)GL_SRC_ALPHA,
			// 7 -,
			(BlendingFactorDest)GL_ONE_MINUS_SRC_ALPHA,
			// 8 -,
			(BlendingFactorDest)GL_DST_ALPHA,
			// 9 -,
			(BlendingFactorDest)GL_ONE_MINUS_DST_ALPHA,
			// 10 GU_FIX
			(BlendingFactorDest)GL_ONE_MINUS_SRC_ALPHA,
		};

		static readonly TextureEnvMode[] TextureEnvModeTranslate = new[]
		{
			TextureEnvMode.Modulate,
			TextureEnvMode.Decal,
			TextureEnvMode.Blend,
			TextureEnvMode.Replace,
			TextureEnvMode.Add,
		};

		public struct GlPixelFormat
		{
			public GuPixelFormats GuPixelFormat;
			public PixelType OpenglPixelType;
		};

		static public readonly GlPixelFormat[] GlPixelFormatList = new GlPixelFormat[]
		{
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_5650, OpenglPixelType = PixelType.UnsignedShort565Reversed },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_5551, OpenglPixelType = PixelType.UnsignedShort1555Reversed },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_4444, OpenglPixelType = PixelType.UnsignedShort4444Reversed },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_8888, OpenglPixelType = PixelType.UnsignedInt8888Reversed },
		};
	}
}
