using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CSharpUtils
{
	public class ProcessUtils
	{
		static public string ExecuteCommand(string Command, string Arguments, string WorkingDirectory = ".")
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
			return proc.StandardOutput.ReadToEnd();
			//proc.WaitForExit();
		}
	}
}
