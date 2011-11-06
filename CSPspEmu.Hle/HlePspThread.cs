using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Threading;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle
{
	public class HlePspThread
	{
		protected GreenThread GreenThread;
		protected CpuThreadState Processor;

		public HlePspThread(CpuThreadState Processor)
		{
			this.Processor = Processor;
		}

		public void Run()
		{
		}
	}
}
