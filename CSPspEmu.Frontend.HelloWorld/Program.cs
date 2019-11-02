using System;
using System.Threading;

namespace CSPspEmu.Frontend.HelloWorld
{
    class Program 
    {
        /*
        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine(GL.GetString(StringName.Version));
        }

        static void Main()
        {
            using (var game = new Program())
            {
                game.Run(60);
            }
        }
        */

        static void Main(string[] args)
        {
            //GL.InitNames();
            //Console.WriteLine(GL.GetString(StringName.Version));
            Console.WriteLine("Hello World!");
            /*
            using (var pspEmulator = new PspEmulator())
            {
                pspEmulator.StartAndLoad("minifire.pbp", GuiRunner: (emulator) =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(10_000));
                });
            }
            */
        }
    }
}
    