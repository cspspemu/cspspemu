using System;

namespace CSPspEmu.Core.Gpu.State
{
    public enum TransformModeEnum : byte
    {
        Normal = 0,
        Raw = 1,
    }

    public enum GuPrimitiveType : byte
    {
        Points = 0,
        Lines = 1,
        LineStrip = 2,
        Triangles = 3,
        TriangleStrip = 4,
        TriangleFan = 5,
        Sprites = 6,
        ContinuePreviousPrim = 7,
    }

    public enum TextureColorComponent : byte
    {
        Rgb = 0, // GU_TCC_RGB
        Rgba = 1, // GU_TCC_RGBA
    }

    public enum TextureEffect : byte
    {
        Modulate = 0, // GU_TFX_MODULATE
        Decal = 1, // GU_TFX_DECAL
        Blend = 2, // GU_TFX_BLEND
        Replace = 3, // GU_TFX_REPLACE
        Add = 4, // GU_TFX_ADD
    }

    public enum TextureFilter : byte
    {
        Nearest = 0,
        Linear = 1,

        NearestMipmapNearest = 4,
        LinearMipmapNearest = 5,
        NearestMipmapLinear = 6,
        LinearMipmapLinear = 7,
    }

    public enum WrapMode : byte
    {
        Repeat = 0,
        Clamp = 1,
    }

    public enum LogicalOperationEnum : byte
    {
        Clear = 0,
        And = 1,
        AndReverse = 2,
        Copy = 3,
        AndInverted = 4,
        Noop = 5,
        Xor = 6,
        Or = 7,
        NotOr = 8,
        Equiv = 9,
        Inverted = 10,
        OrReverse = 11,
        CopyInverted = 12,
        OrInverted = 13,
        NotAnd = 14,
        Set = 15
    }

    public enum StencilOperationEnum : byte
    {
        Keep = 0,
        Zero = 1,
        Replace = 2,
        Invert = 3,
        Increment = 4,
        Decrement = 5,
    }

    public enum ShadingModelEnum : byte
    {
        Flat = 0,
        Smooth = 1,
    }

    [Flags]
    public enum ClearBufferSet : byte
    {
        ColorBuffer = 1,
        StencilBuffer = 2,
        DepthBuffer = 4,
        FastClear = 16
    }

    public enum BlendingOpEnum : byte
    {
        Add = 0,
        Substract = 1,
        ReverseSubstract = 2,
        Min = 3,
        Max = 4,
        Abs = 5,
    }

    public enum GuBlendingFactorSource : byte
    {
        // Source
        GU_SRC_COLOR = 0,
        GU_ONE_MINUS_SRC_COLOR = 1,
        GU_SRC_ALPHA = 2,
        GU_ONE_MINUS_SRC_ALPHA = 3,

        // Both?
        GU_FIX = 10
    }

    public enum GuBlendingFactorDestination : byte
    {
        // Dest
        GU_DST_COLOR = 0,
        GU_ONE_MINUS_DST_COLOR = 1,
        GU_DST_ALPHA = 4,
        GU_ONE_MINUS_DST_ALPHA = 5,

        // Both?
        GU_FIX = 10
    }

    public enum TestFunctionEnum : byte
    {
        Never = 0,
        Always = 1,
        Equal = 2,
        NotEqual = 3,
        Less = 4,
        LessOrEqual = 5,
        Greater = 6,
        GreaterOrEqual = 7,
    }

    public enum FrontFaceDirectionEnum : byte
    {
        CounterClockWise = 0,
        ClockWise = 1
    }

    public enum ColorTestFunctionEnum : byte
    {
        GU_NEVER,
        GU_ALWAYS,
        GU_EQUAL,
        GU_NOTEQUAL,
    }

    [Flags]
    public enum LightComponentsSet : byte
    {
        Ambient = 1,
        Diffuse = 2,
        Specular = 4,
        AmbientAndDiffuse = Ambient | Diffuse,
        DiffuseAndSpecular = Diffuse | Specular,
        UnknownLightComponent = 8,
    }

    public enum LightTypeEnum : byte
    {
        Directional = 0,
        PointLight = 1,
        SpotLight = 2,
    }

    public enum LightModelEnum : byte
    {
        SingleColor = 0,
        SeparateSpecularColor = 1,
    }

    public enum TextureMapMode : byte
    {
        GU_TEXTURE_COORDS = 0,
        GU_TEXTURE_MATRIX = 1,
        GU_ENVIRONMENT_MAP = 2,
    }

    public enum TextureProjectionMapMode : byte
    {
        /// <summary>
        /// TMAP_TEXTURE_PROJECTION_MODE_POSITION
        /// 3 texture components
        /// </summary>
        GU_POSITION = 0,

        /// <summary>
        /// TMAP_TEXTURE_PROJECTION_MODE_TEXTURE_COORDINATES
        /// 2 texture components
        /// </summary>
        GU_UV = 1,

        /// <summary>
        /// TMAP_TEXTURE_PROJECTION_MODE_NORMALIZED_NORMAL
        /// 3 texture components
        /// </summary>
        GU_NORMALIZED_NORMAL = 2,

        /// <summary>
        /// TMAP_TEXTURE_PROJECTION_MODE_NORMAL
        /// 3 texture components
        /// </summary>
        GU_NORMAL = 3,
    }
}