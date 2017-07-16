using System;
using System.Diagnostics;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <param name="workingDirectory"></param>
        /// <returns></returns>
        public static ProcessResult ExecuteCommand(string command, string arguments, string workingDirectory = ".")
        {
            var proc = new Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = command;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.ErrorDialog = false;
            proc.StartInfo.WorkingDirectory = workingDirectory;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            var outputString = proc.StandardOutput.ReadToEnd();
            var errorString = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            var exitCode = proc.ExitCode;

            return new ProcessResult()
            {
                OutputString = outputString,
                ErrorString = errorString,
                ExitCode = exitCode,
            };
            //proc.WaitForExit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationPath"></param>
        /// <param name="applicationArguments"></param>
        /// <returns></returns>
        public static ProcessResult RunProgramInBackgroundAsRoot(string applicationPath, string applicationArguments)
        {
            // This snippet needs the "System.Diagnostics"
            // library


            // Application path and command line arguments
            //string ApplicationPath = ApplicationPaths.ExecutablePath;
            //string ApplicationArguments = "/associate";

            //Console.WriteLine(ExecutablePath);

            // Create a new process object
            var processObj = new Process();

            processObj.StartInfo = new ProcessStartInfo()
            {
                // StartInfo contains the startup information of the new process
                FileName = applicationPath,
                Arguments = applicationArguments,

                UseShellExecute = true,
                Verb = "runas",

                // These two optional flags ensure that no DOS window appears
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                //RedirectStandardOutput = true,
                RedirectStandardOutput = false,
            };

            var OutputString = "";
            var ErrorString = "";
            Exception exception = null;
            // Wait that the process exits
            try
            {
                // Start the process
                processObj.Start();

                //OutputString = ProcessObj.StandardOutput.ReadToEnd();
                //ErrorString = ProcessObj.StandardError.ReadToEnd();
                processObj.WaitForExit();
            }
            catch (Exception ex2)
            {
                exception = ex2;
                Console.WriteLine(exception);
            }

            return new ProcessResult()
            {
                OutputString = OutputString,
                ErrorString = ErrorString,
                Exception = exception,
                ExitCode = processObj.ExitCode,
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// 
        /// </summary>
        public string OutputString;

        /// <summary>
        /// 
        /// </summary>
        public string ErrorString;

        /// <summary>
        /// 
        /// </summary>
        public int ExitCode;

        /// <summary>
        /// 
        /// </summary>
        public Exception Exception;

        /// <summary>
        /// 
        /// </summary>
        public bool Success => Exception == null && ExitCode == 0;
    }
}