using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Sandbox
{
	class Program
	{
		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Common_Intermediate_Language"/>
		/// <see cref="http://en.wikipedia.org/wiki/List_of_CIL_instructions"/>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Console.WriteLine("[.]");
			var Instructions = InstructionTable.ALL();
			Console.WriteLine("[.]");
			Console.ReadKey();
		}
	}
}
