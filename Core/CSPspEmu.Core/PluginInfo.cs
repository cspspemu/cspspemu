using System;

namespace CSPspEmu.Core
{
	public class PluginInfo
	{
		/// <summary>
		/// 
		/// </summary>
		public string Name;

		/// <summary>
		/// 
		/// </summary>
		public string Version;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString() => $"Plugin. Name: {Name}, Version: {Version}";
	}
}
