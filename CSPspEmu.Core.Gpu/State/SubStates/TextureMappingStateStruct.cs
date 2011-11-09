using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct TextureMappingStateStruct
	{
		/// <summary>
		/// Texture Mapping Enable (GL_TEXTURE_2D)
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public GpuMatrixStruct Matrix;

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
	}
}
