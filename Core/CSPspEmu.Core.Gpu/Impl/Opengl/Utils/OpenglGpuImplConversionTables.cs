using CSharpPlatform.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Utils
{
    static internal class OpenglGpuImplConversionTables
    {
        static internal readonly int[] StencilOperationTranslate =
        {
            GL.GL_KEEP,
            GL.GL_ZERO,
            GL.GL_REPLACE,
            GL.GL_INVERT,
            GL.GL_INCR,
            GL.GL_DECR,
        };

        static internal readonly int[] StencilFunctionTranslate =
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

        static internal readonly int[] DepthFunctionTranslate =
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

        static internal readonly int[] BlendEquationTranslate =
        {
            GL.GL_FUNC_ADD,
            GL.GL_FUNC_SUBTRACT,
            GL.GL_FUNC_REVERSE_SUBTRACT,
            GL.GL_FUNC_ADD, // MIN
            GL.GL_FUNC_ADD, // MAX
            GL.GL_FUNC_ADD, // ABS
        };

        static internal readonly int[] BlendFuncSrcTranslate =
        {
            GL.GL_SRC_COLOR, // 0 GU_SRC_COLOR,
            GL.GL_ONE_MINUS_SRC_COLOR, // 1 GU_ONE_MINUS_SRC_COLOR,
            GL.GL_SRC_ALPHA, // 2 GU_SRC_ALPHA,
            GL.GL_ONE_MINUS_SRC_ALPHA, // 3 GU_ONE_MINUS_SRC_ALPHA,
            GL.GL_DST_ALPHA, // 4 -,
            GL.GL_ONE_MINUS_DST_ALPHA, // 5 -,
            GL.GL_SRC_ALPHA, // 6 -,
            GL.GL_ONE_MINUS_SRC_ALPHA, // 7 -,
            GL.GL_DST_ALPHA, // 8 -,
            GL.GL_ONE_MINUS_DST_ALPHA, // 9 -,
            GL.GL_SRC_ALPHA, // 10 GU_FIX
        };

        static internal readonly int[] BlendFuncDstTranslate =
        {
            GL.GL_DST_COLOR, // 0 GU_DST_COLOR,
            GL.GL_ONE_MINUS_DST_COLOR, // 1 GU_ONE_MINUS_DST_COLOR,
            GL.GL_DST_ALPHA, // 2 -,
            GL.GL_ONE_MINUS_DST_ALPHA, // 3 -,
            GL.GL_DST_ALPHA, // 4 GU_DST_ALPHA,
            GL.GL_ONE_MINUS_DST_ALPHA, // 5 GU_ONE_MINUS_DST_ALPHA,
            GL.GL_SRC_ALPHA, // 6 -,
            GL.GL_ONE_MINUS_SRC_ALPHA, // 7 -,
            GL.GL_DST_ALPHA, // 8 -,
            GL.GL_ONE_MINUS_DST_ALPHA, // 9 -,
            GL.GL_ONE_MINUS_SRC_ALPHA, // 10 GU_FIX
        };

        //static internal readonly TextureEnvMode[] TextureEnvModeTranslate = new[]
        //{
        //	TextureEnvMode.Modulate, // GU_TFX_MODULATE
        //	TextureEnvMode.Decal,    // GU_TFX_DECAL
        //	TextureEnvMode.Blend,    // GU_TFX_BLEND
        //	TextureEnvMode.Replace,  // GU_TFX_REPLACE
        //	TextureEnvMode.Add,      // GU_TFX_ADD
        //};

        //internal struct GlPixelFormat
        //{
        //	public GuPixelFormats GuPixelFormat;
        //	public PixelType OpenglPixelType;
        //};
        //
        //static internal readonly GlPixelFormat[] GlPixelFormatList = new GlPixelFormat[]
        //{
        //	new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_5650, OpenglPixelType = PixelType.UnsignedShort565Reversed },
        //	new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_5551, OpenglPixelType = PixelType.UnsignedShort1555Reversed },
        //	new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_4444, OpenglPixelType = PixelType.UnsignedShort4444Reversed },
        //	new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_8888, OpenglPixelType = PixelType.UnsignedInt8888Reversed },
        //};
    }
}