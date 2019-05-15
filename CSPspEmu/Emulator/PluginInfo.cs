namespace CSPspEmu.Core
{
    public class PluginInfo
    {
        public string Name;
        public string Version;

        public PluginInfo()
        {
        }

        public PluginInfo(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public override string ToString() => $"Plugin. Name: {Name}, Version: {Version}";
    }
}