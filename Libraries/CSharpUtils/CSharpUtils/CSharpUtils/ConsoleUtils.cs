using CSharpUtils;
using System;
using System.IO;

static public class TextWriterExtensions
{
    public static void WriteLineColored(this TextWriter TextWriter, ConsoleColor ConsoleColor, string Format,
        params Object[] Args)
    {
        ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor, () => { TextWriter.WriteLine(Format, Args); });
    }
}

namespace CSharpUtils
{
    public class ConsoleUtils
    {
        public static void SaveRestoreConsoleColor(ConsoleColor Color, Action Action)
        {
            SaveRestoreConsoleState(() =>
            {
                Console.ForegroundColor = Color;
                Action();
            });
        }

        public static void SaveRestoreConsoleState(Action Action)
        {
            lock (Console.Out)
            {
                var BackBackgroundColor = Console.BackgroundColor;
                var BackForegroundColor = Console.ForegroundColor;
                try
                {
                    Action();
                }
                finally
                {
                    Console.BackgroundColor = BackBackgroundColor;
                    Console.ForegroundColor = BackForegroundColor;
                }
            }
        }

        public static String CaptureError(Action Action, bool Capture = true)
        {
            if (Capture)
            {
                var OldOut = Console.Error;
                var StringWriter = new StringWriter();
                try
                {
                    Console.SetError(StringWriter);
                    Action();
                }
                finally
                {
                    Console.SetError(OldOut);
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

        public static String CaptureOutput(Action Action, bool Capture = true)
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