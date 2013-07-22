using System;

namespace CSPspEmu.Hle
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class HleInvalidAsNullAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class HleInvalidAsInvalidPointerAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class HlePspFunctionAttribute : Attribute
	{
		/// <summary>
		/// ID for functions. It was initially the first 32 bits of the SHA-1 of the name of the function.
		/// </summary>
		public uint NID;

		/// <summary>
		/// Firmware Version where the function starts to appear.
		/// </summary>
		public uint FirmwareVersion = 150;

		/// <summary>
		/// Avoids being logged.
		/// </summary>
		public bool SkipLog = false;

		/// <summary>
		/// Avoids being executed inside an interrupt.
		/// </summary>
		public bool CheckInsideInterrupt = true;

		/// <summary>
		/// Optional name.
		/// </summary>
		public string Name = "";
	}
}
