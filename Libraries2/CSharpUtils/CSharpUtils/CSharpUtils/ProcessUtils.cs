using System;
using System.Diagnostics;

namespace CSharpUtils
{
	public class ProcessUtils
	{
		public static ProcessResult ExecuteCommand(string Command, string Arguments, string WorkingDirectory = ".")
		{
			var proc = new Process();
			proc.EnableRaisingEvents = false;
			proc.StartInfo.FileName = Command;
			proc.StartInfo.Arguments = Arguments;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.ErrorDialog = false;
			proc.StartInfo.WorkingDirectory = WorkingDirectory;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.RedirectStandardInput = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start();

			var OutputString = proc.StandardOutput.ReadToEnd();
			var ErrorString = proc.StandardError.ReadToEnd();
			proc.WaitForExit();
			var ExitCode = proc.ExitCode;
			
			return new ProcessResult()
			{
				OutputString = OutputString,
				ErrorString = ErrorString,
				ExitCode = ExitCode,
			};
			//proc.WaitForExit();
		}

		public static ProcessResult RunProgramInBackgroundAsRoot(string ApplicationPath, string ApplicationArguments)
		{
			// This snippet needs the "System.Diagnostics"
			// library


			// Application path and command line arguments
			//string ApplicationPath = ApplicationPaths.ExecutablePath;
			//string ApplicationArguments = "/associate";

			//Console.WriteLine(ExecutablePath);

			// Create a new process object
			Process ProcessObj = new Process();

			ProcessObj.StartInfo = new ProcessStartInfo()
			{
				// StartInfo contains the startup information of the new process
				FileName = ApplicationPath,
				Arguments = ApplicationArguments,

				UseShellExecute = true,
				Verb = "runas",

				// These two optional flags ensure that no DOS window appears
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				//RedirectStandardOutput = true,
				RedirectStandardOutput = false,
			};

			string OutputString = "";
			string ErrorString = "";
			Exception Exception = null;
			// Wait that the process exits
			try
			{
				// Start the process
				ProcessObj.Start();

				//OutputString = ProcessObj.StandardOutput.ReadToEnd();
				//ErrorString = ProcessObj.StandardError.ReadToEnd();
				ProcessObj.WaitForExit();
			}
			catch (Exception _Exception)
			{
				Exception = _Exception;
				Console.WriteLine(Exception);
			}

			return new ProcessResult()
			{
				OutputString = OutputString,
				ErrorString = ErrorString,
				Exception = Exception,
				ExitCode = ProcessObj.ExitCode,
			};
		}
	}

	public class ProcessResult
	{
		public string OutputString;
		public string ErrorString;
		public int ExitCode;
		public Exception Exception;
		public bool Success { get { return Exception == null && ExitCode == 0; } }
	}
}
