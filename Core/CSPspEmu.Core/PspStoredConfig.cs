using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using CSharpUtils;

namespace CSPspEmu.Core
{
	public class ControllerConfig
	{
		public string DigitalUp = "Up";
		public string DigitalDown = "Down";
		public string DigitalLeft = "Left";
		public string DigitalRight = "Right";

		public string AnalogUp = "I";
		public string AnalogDown = "K";
		public string AnalogLeft = "J";
		public string AnalogRight = "L";

		public string SelectButton = "Space";
		public string StartButton = "Return";

		public string SquareButton = "A";
		public string CircleButton = "D";
		public string TriangleButton = "W";
		public string CrossButton = "S";

		public string LeftTriggerButton = "Q";
		public string RightTriggerButton = "E";
	}

	public class PspStoredConfig
	{
		public static Logger Logger = Logger.GetLogger("Config");

		public bool EnableSmaa = false;

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
		public int DisplayScale = 2;

		/// <summary>
		/// 
		/// </summary>
		public int RenderScale = 2;

		/// <summary>
		/// 
		/// </summary>
		public bool UseFastMemory = false;

		/// <summary>
		/// 
		/// </summary>
		public bool EnableAstOptimizations = true;

		/// <summary>
		/// 
		/// </summary>
		public ControllerConfig ControllerConfig = new ControllerConfig();

		/// <summary>
		/// 
		/// </summary>
		public List<string> RecentFiles = new List<string>();

		/// <summary>
		/// 
		/// </summary>
		public string IsosPath = null;

		/// <summary>
		/// 
		/// </summary>
		public bool ScaleTextures = false;

		#region Serializing
		private static XmlSerializer _serializer;

		private PspStoredConfig()
		{
		}

		private static string ConfigFilePath
		{
			get
			{
				return ApplicationPaths.MemoryStickRootFolder + "/EmulatorConfig.xml";
			}
		}

		private static object Lock = new object();

		public static PspStoredConfig Load()
		{
			lock (Lock)
			{
				try
				{
					if (_serializer == null)
					{
						_serializer = XmlSerializer.FromTypes(new[] { typeof(PspStoredConfig) })[0];
					}

					using (var stream = File.OpenRead(ConfigFilePath))
					{
						return (PspStoredConfig)_serializer.Deserialize(stream);
					}
				}
				catch (Exception exception)
				{
					Logger.Error(exception);
					return new PspStoredConfig();
				}
			}
		}

		public void Save()
		{
			try
			{
				lock (Lock)
				{
					using (var stream = File.Open(ConfigFilePath, FileMode.Create, FileAccess.Write))
					{
						_serializer.Serialize(stream, this);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}
		}
		#endregion
	}
}
