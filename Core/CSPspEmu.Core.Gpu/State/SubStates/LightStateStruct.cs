using Mono.Simd;
using System.Runtime.InteropServices;
namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AttenuationStruct
	{
		public float Constant;
		public float Linear;
		public float Quadratic;
	}

	public struct Vector4fRef
	{
		public float X, Y, Z, W;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
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
		public Vector4fRef Position;

		/// <summary>
		/// 
		/// </summary>
		public Vector4fRef SpotDirection;

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
