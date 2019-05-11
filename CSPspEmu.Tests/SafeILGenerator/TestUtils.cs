using System;
using System.IO;

namespace SafeILGenerator.Tests
{
    public class TestUtils
    {
        public static string CaptureOutput(Action action, bool capture = true)
        {
            if (capture)
            {
                var oldOut = Console.Out;
                var stringWriter = new StringWriter();
                try
                {
                    Console.SetOut(stringWriter);
                    action();
                }
                finally
                {
                    Console.SetOut(oldOut);
                }
                try
                {
                    return stringWriter.ToString();
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                action();
                return "";
            }
        }
    }
}