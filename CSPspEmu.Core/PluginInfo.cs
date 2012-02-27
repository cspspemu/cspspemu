using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		public override string ToString()
		{
			return String.Format("Plugin. Name: {0}, Version: {1}", Name, Version);
		}
	}
}
