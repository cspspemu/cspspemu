using System;

namespace CSPspEmu.Compat
{
    public class Application
    {
        public static string ExecutablePath => AppDomain.CurrentDomain.BaseDirectory;
    }
}