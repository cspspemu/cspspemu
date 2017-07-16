using CSharpUtils;
using System;
using System.IO;

/// <summary>
/// 
/// </summary>
public static class TextWriterExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="textWriter"></param>
    /// <param name="consoleColor"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public static void WriteLineColored(this TextWriter textWriter, ConsoleColor consoleColor, string format,
        params object[] args)
    {
        ConsoleUtils.SaveRestoreConsoleColor(consoleColor, () => { textWriter.WriteLine(format, args); });
    }
}

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class ConsoleUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="action"></param>
        public static void SaveRestoreConsoleColor(ConsoleColor color, Action action)
        {
            SaveRestoreConsoleState(() =>
            {
                Console.ForegroundColor = color;
                action();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public static void SaveRestoreConsoleState(Action action)
        {
            lock (Console.Out)
            {
                var backBackgroundColor = Console.BackgroundColor;
                var backForegroundColor = Console.ForegroundColor;
                try
                {
                    action();
                }
                finally
                {
                    Console.BackgroundColor = backBackgroundColor;
                    Console.ForegroundColor = backForegroundColor;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="capture"></param>
        /// <returns></returns>
        public static string CaptureError(Action action, bool capture = true)
        {
            if (!capture)
            {
                action();
                return "";
            }
            var oldOut = Console.Error;
            var stringWriter = new StringWriter();
            try
            {
                Console.SetError(stringWriter);
                action();
            }
            finally
            {
                Console.SetError(oldOut);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="capture"></param>
        /// <returns></returns>
        public static string CaptureOutput(Action action, bool capture = true)
        {
            if (!capture)
            {
                action();
                return "";
            }
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
    }
}