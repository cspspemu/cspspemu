using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static System.String;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Logger
    {
        /// <summary>
        /// 
        /// </summary>
        public enum Level
        {
            /// <summary>
            /// 
            /// </summary>
            Notice,

            /// <summary>
            /// 
            /// </summary>
            Info,

            /// <summary>
            /// 
            /// </summary>
            Warning,

            /// <summary>
            /// 
            /// </summary>
            Unimplemented,

            /// <summary>
            /// 
            /// </summary>
            Error,

            /// <summary>
            /// 
            /// </summary>
            Fatal,
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }

        //private bool Enabled = true;
        private bool Enabled = false;

        private static readonly Dictionary<string, Logger> Loggers = new Dictionary<string, Logger>();

        internal Logger()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Logger CreateAnonymousLogger()
        {
            return new Logger();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Logger GetLogger(string name)
        {
            lock (Loggers)
            {
                if (!Loggers.ContainsKey(name))
                {
                    Loggers[name] = new Logger()
                    {
                        Name = name,
                    };
                }

                return Loggers[name];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static event Action<string, Level, string, StackFrame> OnGlobalLog;

        /// <summary>
        /// 
        /// </summary>
        public event Action<Level, string, StackFrame> OnLog;

        private Logger Log(Level level, object format, params object[] Params)
        {
            if (Enabled || OnGlobalLog != null)
            {
                var stackTrace = new StackTrace();
                StackFrame stackFrame = null;
                var stackFrames = stackTrace.GetFrames();
                if (stackFrames != null)
                    foreach (var frame in stackFrames)
                    {
                        if (frame.GetMethod().DeclaringType != typeof(Logger))
                        {
                            stackFrame = frame;
                            break;
                        }
                    }

                if (Enabled)
                {
                    OnLog?.Invoke(level, Format(format.ToString(), Params), stackFrame);
                }

                OnGlobalLog?.Invoke(Name, level, Format(format.ToString(), Params), stackFrame);
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public Logger Notice(object format, params object[] Params)
        {
            return Log(Level.Notice, format, Params);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public Logger Info(object format, params object[] Params)
        {
            return Log(Level.Info, format, Params);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public Logger Warning(object format, params object[] Params)
        {
            return Log(Level.Warning, format, Params);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public Logger Unimplemented(object format, params object[] Params)
        {
            return Log(Level.Unimplemented, format, Params);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public Logger Error(object format, params object[] Params)
        {
            return Log(Level.Error, format, Params);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public Logger Fatal(object format, params object[] Params)
        {
            return Log(Level.Fatal, format, Params);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TimeSpan Measure(Action action)
        {
            var start = DateTime.UtcNow;
            action();
            var end = DateTime.UtcNow;
            return end - start;
        }

        /// <summary>
        /// 
        /// </summary>
        public class Stopwatch
        {
            protected List<DateTime> DateTimeList = new List<DateTime>();
            protected DateTime LastDateTime;

            /// <summary>
            /// 
            /// </summary>
            public Stopwatch()
            {
                Start();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public Stopwatch Start()
            {
                LastDateTime = DateTime.UtcNow;
                return this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public TimeSpan Tick()
            {
                var now = DateTime.UtcNow;
                DateTimeList.Add(now);
                try
                {
                    return now - LastDateTime;
                }
                finally
                {
                    LastDateTime = now;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var timeSpans = new List<TimeSpan>();
                for (var n = 1; n < DateTimeList.Count; n++)
                {
                    timeSpans.Add(DateTimeList[n] - DateTimeList[n - 1]);
                }

                return
                    $"Logger.Stopwatch({Join(",", timeSpans.Select(item => $"{(int) item.TotalMilliseconds} ms"))})";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void TryCatch(Action action)
        {
            if (Debugger.IsAttached)
            {
                action();
            }
            else
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Error(e);
                }
            }
        }
    }
}