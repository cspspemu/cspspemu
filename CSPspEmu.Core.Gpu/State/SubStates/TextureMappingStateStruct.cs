using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public enum TextureMapMode : uint
	{
		GU_TEXTURE_COORDS = 0,
		GU_TEXTURE_MATRIX = 1,
		GU_ENVIRONMENT_MAP = 2,
	}

	public enum TextureProjectionMapMode : uint
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
		public ColorfStruct TextureEnviromentColor;

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
				case SubStates.TextureMapMode.GU_TEXTURE_COORDS:
					break;
				case SubStates.TextureMapMode.GU_TEXTURE_MATRIX:
					switch (TextureProjectionMapMode)
					{
						case SubStates.TextureProjectionMapMode.GU_NORMAL:
							Components = 3;
							break;
						case SubStates.TextureProjectionMapMode.GU_NORMALIZED_NORMAL:
							Components = 3;
							break;
						case SubStates.TextureProjectionMapMode.GU_POSITION:
							Components = 3;
							break;
						case SubStates.TextureProjectionMapMode.GU_UV:
							Components = 2;
							break;
					}
					break;
				case SubStates.TextureMapMode.GU_ENVIRONMENT_MAP:
					break;
			}
			return Components;
		}
	}
}
