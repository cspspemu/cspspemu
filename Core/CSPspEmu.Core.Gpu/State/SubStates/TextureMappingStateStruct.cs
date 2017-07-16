using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureMappingStateStruct
    {
        /// <summary>
        /// Texture Mapping Enable (GL_TEXTURE_2D)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public GpuMatrix4x4Struct Matrix;

        /// <summary>
        /// 
        /// </summary>
        public ColorbStruct TextureEnviromentColor;

        /// <summary>
        /// 
        /// </summary>
        public TextureStateStruct TextureState;

        /// <summary>
        /// 
        /// </summary>
        public ClutStateStruct UploadedClutState;

        /// <summary>
        /// 
        /// </summary>
        public ClutStateStruct ClutState;

        /// <summary>
        /// 
        /// </summary>
        public TextureMapMode TextureMapMode;

        /// <summary>
        /// 
        /// </summary>
        public TextureProjectionMapMode TextureProjectionMapMode;

        public short ShadeU;
        public short ShadeV;
        public TextureLevelMode LevelMode;
        public float MipmapBias;
        public float SlopeLevel;

        public byte GetTextureComponentsCount()
        {
            byte Components = 2;
            switch (TextureMapMode)
            {
                case TextureMapMode.GU_TEXTURE_COORDS:
                    break;
                case TextureMapMode.GU_TEXTURE_MATRIX:
                    switch (TextureProjectionMapMode)
                    {
                        case TextureProjectionMapMode.GU_NORMAL:
                            Components = 3;
                            break;
                        case TextureProjectionMapMode.GU_NORMALIZED_NORMAL:
                            Components = 3;
                            break;
                        case TextureProjectionMapMode.GU_POSITION:
                            Components = 3;
                            break;
                        case TextureProjectionMapMode.GU_UV:
                            Components = 2;
                            break;
                    }
                    break;
                case TextureMapMode.GU_ENVIRONMENT_MAP:
                    break;
            }
            return Components;
        }
    }
}