namespace CSPspEmu.Core
{
    public class PluginInfo
    {
        public string Name;
        public string Version;
        public override string ToString() => $"Plugin. Name: {Name}, Version: {Version}";
    }
}