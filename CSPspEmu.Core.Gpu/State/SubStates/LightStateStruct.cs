using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct AttenuationStruct
	{
		public float Constant;
		public float Linear;
		public float Quadratic;
	}

	public enum LightTypeEnum
	{
		Directional = 0,
		PointLight = 1,
		SpotLight = 2,
	}

	public enum LightModelEnum
	{
		SingleColor = 0,
		SeparateSpecularColor = 1,
	}

	public struct LightStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public LightTypeEnum Type;

		/// <summary>
		/// 
		/// </summary>
		public LightModelEnum Kind;

		/// <summary>
		/// 
		/// </summary>
		public GpuVectorStruct Position;

		/// <summary>
		/// 
		/// </summary>
		public GpuVectorStruct SpotDirection;

		/// <summary>
		/// 
		/// </summary>
		public AttenuationStruct Attenuation;

		/// <summary>
		/// 
		/// </summary>
		public float SpotExponent;

		/// <summary>
		/// 
		/// </summary>
		public float SpotCutoff;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct AmbientColor;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct DiffuseColor;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct SpecularColor;
	}
}
