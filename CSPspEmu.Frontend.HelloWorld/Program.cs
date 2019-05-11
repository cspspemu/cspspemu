using System;
using System.IO;
using System.Threading;
using CSPspEmu.Hle.Modules.emulator;

namespace CSPspEmu.Frontend.HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            using (var pspEmulator = new PspEmulator())
            {
                pspEmulator.StartAndLoad("minifire.pbp", GuiRunner: (emulator) =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(10_000));
                });
            }
        }
    }
}