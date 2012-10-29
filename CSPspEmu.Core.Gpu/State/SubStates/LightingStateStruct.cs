using System;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[Flags]
	public enum LightComponentsSet {
		Ambient = 1,
		Diffuse = 2,
		Specular = 4,
		AmbientAndDiffuse = Ambient | Diffuse,
		DiffuseAndSpecular = Diffuse | Specular,
		UnknownLightComponent = 8,
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LightingStateStruct
	{
		/// <summary>
		/// Lighting Enable (GL_LIGHTING)
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct AmbientModelColor;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct DiffuseModelColor;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct SpecularModelColor;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct EmissiveModelColor;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct AmbientLightColor;

		/// <summary>
		/// 
		/// </summary>
		public float SpecularPower;

		/// <summary>
		/// 
		/// </summary>
		public LightStateStruct Light0, Light1, Light2, Light3;

		/// <summary>
		/// 
		/// </summary>
		public LightComponentsSet MaterialColorComponents;

		/// <summary>
		/// 
		/// </summary>
		public LightModelEnum LightModel;
	}
}
