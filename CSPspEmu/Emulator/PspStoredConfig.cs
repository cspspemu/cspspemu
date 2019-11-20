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

        public DateTime LastCheckedTime;
        public bool LimitVerticalSync = true;
        public int DisplayScale = 2;
        public int RenderScale = 2;
        public bool UseFastMemory = true;
        public bool EnableAstOptimizations = true;
        public ControllerConfig ControllerConfig = new ControllerConfig();
        public List<string> RecentFiles = new List<string>();
        public string IsosPath = null;
        public bool ScaleTextures = false;

        #region Serializing

        private static XmlSerializer Serializer;

        private PspStoredConfig()
        {
        }

        private static string ConfigFilePath => ApplicationPaths.MemoryStickRootFolder + "/EmulatorConfig.xml";

        private static object Lock = new object();

        public static PspStoredConfig Load()
        {
            lock (Lock)
            {
                try
                {
                    if (Serializer == null)
                    {
                        Serializer = XmlSerializer.FromTypes(new[] {typeof(PspStoredConfig)})[0];
                    }

                    using (var Stream = File.OpenRead(ConfigFilePath))
                    {
                        return (PspStoredConfig) Serializer.Deserialize(Stream);
                    }
                }
                catch (Exception Exception)
                {
                    Logger.Error(Exception);
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
                    using (var Stream = File.Open(ConfigFilePath, FileMode.Create, FileAccess.Write))
                    {
                        Serializer.Serialize(Stream, this);
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.Error(Exception);
            }
        }

        #endregion
    }
}