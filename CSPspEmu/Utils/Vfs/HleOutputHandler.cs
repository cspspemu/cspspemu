using System;

namespace CSPspEmu.Hle
{
    public class HleOutputHandler
    {
        public HleOutputHandler()
        {
            Console.WriteLine($"Built HleOutputHandler! {GetType()}");
        }
        
        public virtual void Output(string Output)
        {
            Console.WriteLine("     {0}", Output);
        }
    }
}