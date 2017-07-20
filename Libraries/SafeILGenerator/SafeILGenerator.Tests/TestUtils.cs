using System;
using System.IO;

namespace SafeILGenerator.Tests
{
    public class TestUtils
    {
        public static string CaptureOutput(Action Action, bool Capture = true)
        {
            if (Capture)
            {
                var OldOut = Console.Out;
                var StringWriter = new StringWriter();
                try
                {
                    Console.SetOut(StringWriter);
                    Action();
                }
                finally
                {
                    Console.SetOut(OldOut);
                }
                try
                {
                    return StringWriter.ToString();
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                Action();
                return "";
            }
        }
    }
}