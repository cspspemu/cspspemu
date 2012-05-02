using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	public class ProcessUtils
	{
		static public string ExecuteCommand(string Command, string Arguments)
		{
			System.Diagnostics.Process proc = new System.Diagnostics.Process();
			proc.EnableRaisingEvents = false;
			proc.StartInfo.FileName = Command;
			proc.StartInfo.Arguments = Arguments;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.ErrorDialog = false;
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
