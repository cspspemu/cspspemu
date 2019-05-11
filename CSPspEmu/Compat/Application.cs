using System;

namespace CSPspEmu.Compat
{
    public class Application
    {
        static public string ExecutablePath => AppDomain.CurrentDomain.BaseDirectory;
    }
}