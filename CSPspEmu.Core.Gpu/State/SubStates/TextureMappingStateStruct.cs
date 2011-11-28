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
		GU_POSITION = 0,
		GU_UV = 1,
		GU_NORMALIZED_NORMAL = 2,
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
	}
}
