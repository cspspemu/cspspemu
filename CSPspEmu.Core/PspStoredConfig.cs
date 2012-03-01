using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace CSPspEmu.Core
{
	public partial class PspStoredConfig
	{
		/// <summary>
		/// 
		/// </summary>
		public DateTime LastCheckedTime;

		/// <summary>
		/// 
		/// </summary>
		public bool LimitVerticalSync = true;

		/// <summary>
		/// 
		/// </summary>
		public int DisplayScale = 1;

		/// <summary>
		/// 
		/// </summary>
		public bool UseFastMemory = false;

		/// <summary>
		/// 
		/// </summary>
		public bool EnableMpeg = false;

		#region Serializing
		static private XmlSerializer Serializer;

		private PspStoredConfig()
		{
		}

		static private string ConfigFilePath
		{
			get
			{
				return ApplicationPaths.MemoryStickRootFolder + "/EmulatorConfig.xml";
			}
		}

		static public PspStoredConfig Load()
		{
			try
			{
				if (Serializer == null)
				{
					Serializer = XmlSerializer.FromTypes(new[] { typeof(PspStoredConfig) })[0];
				}

				using (var Stream = File.OpenRead(ConfigFilePath))
				{
					return (PspStoredConfig)Serializer.Deserialize(Stream);
				}
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return new PspStoredConfig();
			}
		}

		public void Save()
		{
			using (var Stream = File.Open(ConfigFilePath, FileMode.Create, FileAccess.Write))
			{
				Serializer.Serialize(Stream, this);
			}
		}
		#endregion
	}
}
